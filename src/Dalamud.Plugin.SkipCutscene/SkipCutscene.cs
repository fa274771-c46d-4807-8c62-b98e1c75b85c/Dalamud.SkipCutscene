using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Memory.Exceptions;
using Dalamud.Plugin.Services;
using Dalamud.Plugin.SkipCutscene.Views;

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
    public static ICommandManager? CommandManager { get; private set; }

    [PluginService]
    [NotNull]
    public static IChatGui? ChatGui { get; private set; }

    [PluginService]
    [NotNull]
    public static IPluginLog? PluginLog { get; private set; }

    public WindowSystem WindowSystem { get; } = new(nameof(SkipCutscene));

    public CutsceneAddressResolver AddressResolver { get; } = new CutsceneAddressResolver();

    public IReadOnlyDictionary<string, CommandInfo> Commands { get; }

    public SkipCutscene()
    {
        if (!SetupScanner())
        {
            Dispose();
            throw new MemoryReadException("Could not find cutscene offset addresses");
        }

        Commands = new Dictionary<string, CommandInfo>()
        {
            {
                nameof(SanityCheck), new(SanityCheck)
                {
                    HelpMessage = "Roll your sanity check dice."
                }
            }
        };
        var ConfigWindow = new ConfigWindow(Config, Commands);
        var MainWindow = new MainWindow(Config);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Interface.UiBuilder.Draw += WindowSystem.Draw;
        Interface.UiBuilder.OpenMainUi += MainWindow.Toggle;
        Interface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;

        CommandManager.AddHandler($"/{Config.Command}", Commands[nameof(SanityCheck)]);
    }

    public void Dispose()
    {
        SetEnabled(false);
        GC.SuppressFinalize(this);
    }

    private bool SetupScanner()
    {
        AddressResolver.Setup(SigScanner);

        if (AddressResolver.Valid)
        {
            PluginLog.Information("Cutscene Offset Found.");
            SetEnabled(Config.IsEnabled);
            return true;
        }
        PluginLog.Error("Cutscene Offset Not Found.");
        PluginLog.Warning("Plugin Disabling...");
        return false;
    }

    private void SetEnabled(bool isEnable)
    {
        if (!AddressResolver.Valid) return;
        if (isEnable)
        {
            SafeMemory.Write<short>(AddressResolver.Offset1, -28528);
            SafeMemory.Write<short>(AddressResolver.Offset2, -28528);
        }
        else
        {
            SafeMemory.Write<short>(AddressResolver.Offset1, 13173);
            SafeMemory.Write<short>(AddressResolver.Offset2, 6260);
        }
    }

    internal void SanityCheck(string command, string arguments)
    {
        var dice = Rand.Next(50);

        ChatGui.Print(Config.IsEnabled
            ? $"1d100={dice}, verdict = Insane"
            : $"1d100={dice + 50}, verdict = Sane");

        Config.IsEnabled = !Config.IsEnabled;
        SetEnabled(Config.IsEnabled);
        Interface.SavePluginConfig(Config);
    }
}

public class CutsceneAddressResolver : BaseAddressResolver
{
    public bool Valid => Offset1 != nint.Zero && Offset2 != nint.Zero;

    public nint Offset1 { get; private set; }
    public nint Offset2 { get; private set; }

    protected override void Setup64Bit(ISigScanner sig)
    {
        Offset1 = sig.ScanText("75 33 48 8B 0D ?? ?? ?? ?? BA ?? 00 00 00 48 83 C1 10 E8 ?? ?? ?? ?? 83 78");
        Offset2 = sig.ScanText("74 18 8B D7 48 8D 0D");
        SkipCutscene.PluginLog.Information(
            "Offset1: [\"ffxiv_dx11.exe\"+{0}]",
            (Offset1.ToInt64() - Process.GetCurrentProcess().MainModule!.BaseAddress.ToInt64()).ToString("X")
            );
        SkipCutscene.PluginLog.Information(
            "Offset2: [\"ffxiv_dx11.exe\"+{0}]",
            (Offset2.ToInt64() - Process.GetCurrentProcess().MainModule!.BaseAddress.ToInt64()).ToString("X")
            );
    }
}
