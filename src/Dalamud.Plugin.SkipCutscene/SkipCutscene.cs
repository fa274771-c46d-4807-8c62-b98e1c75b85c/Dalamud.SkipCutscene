using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin.SkipCutscene.GameData;
using Dalamud.Plugin.SkipCutscene.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]
namespace Dalamud.Plugin.SkipCutscene;

public class SkipCutscene : IDalamudPlugin
{
    internal Config Config { get; } =
        (Interface.GetPluginConfig() is Config configuration)
            ? configuration
            : new();

    private Random Rand { get; } = new();

    [PluginService]
    [NotNull]
    public static IDalamudPluginInterface? Interface { get; private set; }

    [PluginService]
    [NotNull]
    public static ISigScanner? SigScanner { get; private set; }

    [PluginService]
    [NotNull]
    public static IGameInteropProvider? InteropProvider { get; private set; }

    [PluginService]
    [NotNull]
    public static ICommandManager? CommandManager { get; private set; }

    [PluginService]
    [NotNull]
    public static IChatGui? ChatGui { get; private set; }

    [PluginService]
    [NotNull]
    public static IPluginLog? PluginLog { get; private set; }

    public WindowSystem WindowSystem { get; } = new(nameof(SkipCutscene));

    public Hook<Hooks.CutsceneCreation> CutsceneCreationHook { get; }

    public SkipCutscene()
    {
        CutsceneCreationHook = SetupCutsceneCreationHook();
        var commands = SetupCommands();
        SetupWindowSystem(commands);
    }

    private Hook<Hooks.CutsceneCreation> SetupCutsceneCreationHook()
    {
        try
        {
            Hook<Hooks.CutsceneCreation> hook = null!; // We want the hook to refer to itself.
            hook = Hooks.CreateCutsceneCreationHook(InteropProvider, (foo) =>
            {
                var shouldSkip = true; // todo
                if (shouldSkip)
                {
                    PluginLog.Information("Skipping cutscene creation");
                    return false;
                }
                return hook.Original.Invoke(foo); // always returns true
            });
            if (Config.IsEnabled)
            {
                hook.Enable();
            }

            PluginLog.Debug("Cutscene creation hook made.\n" +
                $"\tAddress: {hook.Address:X}\n" +
                $"\tEnabled: {hook.IsEnabled}");

            return hook;
        }
        catch (Exception)
        {
            Dispose();
            throw;
        }
    }

    private Dictionary<string, CommandInfo> SetupCommands()
    {
        var commands = new Dictionary<string, CommandInfo>()
        {
            {
                nameof(SanityCheck), new(SanityCheck)
                {
                    HelpMessage = "Roll your sanity check dice."
                }
            }
        };
        CommandManager.AddHandler($"/{Config.Command}", commands[nameof(SanityCheck)]);
        return commands;
    }

    private void SetupWindowSystem(IReadOnlyDictionary<string, CommandInfo> commands)
    {
        var ConfigWindow = new ConfigWindow(Config, commands);
        WindowSystem.AddWindow(ConfigWindow);

        Interface.UiBuilder.Draw += WindowSystem.Draw;
        Interface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (CutsceneCreationHook is { })
        {
            CutsceneCreationHook.Disable();
            CutsceneCreationHook.Dispose();
        }
    }

    internal void SanityCheck(string command, string arguments)
    {
        var dice = Rand.Next(50);

        ChatGui.Print(Config.IsEnabled
            ? $"1d100={dice}, verdict = Insane"
            : $"1d100={dice + 50}, verdict = Sane");

        Config.IsEnabled = !Config.IsEnabled;
        Interface.SavePluginConfig(Config);

        if (Config.IsEnabled)
        {
            CutsceneCreationHook.Enable();
        }
        else
        {
            CutsceneCreationHook.Disable();
        }
    }
}