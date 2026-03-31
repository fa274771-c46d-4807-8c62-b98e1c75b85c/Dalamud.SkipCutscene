using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin.SkipCutscene.Extensions;
using Dalamud.Plugin.SkipCutscene.GameData;
using Dalamud.Plugin.SkipCutscene.Views;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]
namespace Dalamud.Plugin.SkipCutscene;

public partial class SkipCutscene
{
    internal Config Config { get; } =
        (Interface.GetPluginConfig() is Config configuration)
            ? configuration
            : new();

    private static Random Rand { get; } = new();

    public IDtrBarEntry DtrEntry { get; private set; }

    internal ConfigWindow ConfigWindow { get; private set; }

    public WindowSystem WindowSystem { get; } = new(nameof(SkipCutscene));

    public Hook<Hooks.CutsceneCreation> CutsceneCreationHook { get; }

    private const string Command = "sc";

    private static SeString Insane { get; } = new SeStringBuilder().AddUiGlow("Insane", 16).Build();
    private static SeString Sane { get; } = new SeStringBuilder().AddUiGlow("Sane", 45).Build();

    private const int LightPartyMemberCount = 4;

    public SkipCutscene()
    {
        CutsceneCreationHook = SetupCutsceneCreationHook();
        SetupCommands();
        SetupWindowSystem();
        SetupDtrBarEntry();
        Framework.Update += OnUpdate;
    }

    private Territory[] ToSkip { get; set; } = [
        Territory.CastrumMeridianum, 
        Territory.Praetorium, 
        Territory.PortaDecumana];

    private bool PreviousWantsToSkip { get; set; }
    private bool WantsToSkip => (Config.AutoSkipOnLightParty && PartyList.Count == LightPartyMemberCount) || Config.IsEnabled;

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        CutsceneCreationHook.Disable();
        CutsceneCreationHook.Dispose();

        DtrEntry.Remove();

        Interface.UiBuilder.Draw -= WindowSystem.Draw;
        Interface.UiBuilder.OpenConfigUi -= ConfigWindow.Toggle;

        Framework.Update -= OnUpdate;
    }


    private Hook<Hooks.CutsceneCreation> SetupCutsceneCreationHook()
    {
        try
        {
            Hook<Hooks.CutsceneCreation> hook = null!; // We want the hook to refer to itself.
            hook = Hooks.CreateCutsceneCreationHook(InteropProvider, (foo) =>
            {
                if (WantsToSkip && ToSkip.Contains((Territory)ClientState.TerritoryType))
                {
                    PluginLog.Information($"Skipping cutscene creation in territory {(Territory)ClientState.TerritoryType}");
                    ChatGui.Print($"Skipping cutscene creation");

                    return false;
                }
                return hook.Original.Invoke(foo);
            });
            hook.Enable();

            PluginLog.Debug("Cutscene creation hook made.\n" +
                $"\tAddress: {hook.Address:X}");

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
        CommandManager.AddHandler($"/{Command}", commands[nameof(SanityCheck)]);
        return commands;
    }

    [MemberNotNull(nameof(ConfigWindow))]
    private void SetupWindowSystem()
    {
        ConfigWindow = new(this);
        WindowSystem.AddWindow(ConfigWindow);

        Interface.UiBuilder.Draw += WindowSystem.Draw;
        Interface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;
    }

    [MemberNotNull(nameof(DtrEntry))]
    private void SetupDtrBarEntry()
    {
        DtrEntry = DtrBar.Get(nameof(SkipCutscene));
        DtrEntry.Shown = Config.ShowInTopBar;
        SetDtrEntryText();
        DtrEntry.OnClick = (e) => { 
            switch (e)
            {
                case { ClickType: MouseClickType.Left }:
                    SanityCheck(string.Empty, string.Empty);
                    break;
                case { ClickType: MouseClickType.Right }:
                    ConfigWindow.Toggle();
                    break;
            }
        };
    }

    private void OnUpdate(IFramework _)
    {
        if (PreviousWantsToSkip != WantsToSkip)
        {
            PreviousWantsToSkip = WantsToSkip;
            SetDtrEntryText();
        }
    }

    private void SetDtrEntryText()
    {
        DtrEntry.Text = new SeStringBuilder()
            .AddIcon(BitmapFontIcon.WatchingCutscene)
            .Add(WantsToSkip ? Sane : Insane)
            .Build();
    }

    internal void SanityCheck(string command, string arguments)
    {
        if (!string.IsNullOrEmpty(command))
        {
            if (!int.TryParse(arguments, out var diceSize))
            {
                diceSize = 100;
            }
            var dice = Rand.Next(diceSize / 2);

            ChatGui.Print(Config.IsEnabled
                ? $"1d{diceSize}={dice}, verdict = {Insane}"
                : $"1d{diceSize}={dice + (diceSize / 2)}, verdict = {Sane}");
        }

        Config.IsEnabled = !Config.IsEnabled;
        Interface.SavePluginConfig(Config);
    }
}