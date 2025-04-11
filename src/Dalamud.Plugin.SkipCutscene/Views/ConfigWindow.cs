using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Dalamud.Plugin.SkipCutscene.Views;

internal class ConfigWindow(
    Config config,
    IReadOnlyDictionary<string, CommandInfo> commands
    ) : Window(nameof(SkipCutscene), ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize), IDisposable
{
    private readonly Config Config = config;
    private readonly IReadOnlyDictionary<string, CommandInfo> Commands = commands;

    public override void Draw()
    {
        bool save = false;

        save &= DrawEnabled();

        save &= DrawCommand();

        if (save)
        {
            SkipCutscene.Interface.SavePluginConfig(Config);
        }
    }

    private bool DrawEnabled()
    {
        var enabled = Config.IsEnabled;
        return ImGui.Checkbox("Enabled", ref enabled);
    }

    private bool DrawCommand()
    {
        var command = Config.Command;
        var oldCommand = command;
        ImGui.SetNextItemWidth(50);
        if (ImGui.InputText("Command to use?", ref command, 250, ImGuiInputTextFlags.CharsNoBlank) &&
            !string.IsNullOrWhiteSpace(command) &&
            (command != oldCommand))
        {
            SkipCutscene.CommandManager.RemoveHandler($"/{oldCommand}");
            SkipCutscene.CommandManager.AddHandler($"/{command}", Commands[nameof(SkipCutscene.SanityCheck)]);

            Config.Command = command;
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
