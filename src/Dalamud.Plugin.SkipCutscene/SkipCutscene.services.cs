using Dalamud.IoC;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Dalamud.Plugin.SkipCutscene;

public partial class SkipCutscene: IDalamudPlugin
{
    [PluginService]
    [NotNull]
    public static IDalamudPluginInterface? Interface { get; private set; }

    [PluginService]
    [NotNull]
    public static IGameInteropProvider? InteropProvider { get; private set; }

    [PluginService]
    [NotNull]
    public static ICommandManager? CommandManager { get; private set; }

    [PluginService]
    [NotNull]
    public static IFramework? Framework { get; private set; }

    [PluginService]
    [NotNull]
    public static IChatGui? ChatGui { get; private set; }

    [PluginService]
    [NotNull]
    public static IPluginLog? PluginLog { get; private set; }

    [PluginService]
    [NotNull]
    public static IClientState? ClientState { get; private set; }

    [PluginService]
    [NotNull]
    public static IPartyList? PartyList { get; private set; }

    [PluginService]
    [NotNull]
    public static IDtrBar? DtrBar { get; private set; }
}
