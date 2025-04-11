using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Dalamud.Plugin.SkipCutscene.Views;

internal class ConfigWindow(
    Config config,
    IReadOnlyDictionary<string, CommandInfo> commands
    ) : Window($"{nameof(SkipCutscene)} configuration"), IDisposable
{
    public override void Draw()
    {
        var command = config.Command;
        var oldCommand = command;
        //SkipCutscene.PluginLog.Logger.Information("{Command}, {OldCommand}", command, oldCommand);

        ImGui.InputText("Command to use?", ref command, 250, ImGuiInputTextFlags.CharsNoBlank);
        if (command != oldCommand)
        {
            SkipCutscene.CommandManager.RemoveHandler($"/{oldCommand}");
            SkipCutscene.CommandManager.AddHandler($"/{command}", commands[nameof(SkipCutscene.SanityCheck)]);

            config.Command = command;
            SkipCutscene.Interface.SavePluginConfig(config);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
