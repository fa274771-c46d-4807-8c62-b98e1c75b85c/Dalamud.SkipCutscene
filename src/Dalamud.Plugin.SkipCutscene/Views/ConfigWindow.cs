using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace Dalamud.Plugin.SkipCutscene.Views;

internal class ConfigWindow(
    SkipCutscene parent
    ) : Window(nameof(SkipCutscene), ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize), IDisposable
{
    private Config Config => parent.Config;

    public override void Draw()
    {
        bool save = false;

        save |= DrawEnabled();

        save |= DrawAutoskip();

        save |= DrawShowDtr();

        if (save)
        {
            SkipCutscene.Interface.SavePluginConfig(Config);
        }
    }

    private bool DrawEnabled()
    {
        var enabled = Config.IsEnabled;
        if (ImGui.Checkbox("Enabled", ref enabled) && 
            (enabled != Config.IsEnabled))
        {
            Config.IsEnabled = enabled;
            return true;
        }
        return false;
    }

    private bool DrawAutoskip()
    {
        var enabled = Config.AutoSkipOnLightParty;
        if (ImGui.Checkbox("Automatically skip when in a premade light party", ref enabled) && enabled != Config.AutoSkipOnLightParty)
        {
            Config.AutoSkipOnLightParty = enabled;
            return true;
        }
        return false;
    }

    private bool DrawShowDtr()
    {
        var enabled = Config.ShowInTopBar;
        if (ImGui.Checkbox("Show in top bar", ref enabled) && enabled != Config.ShowInTopBar)
        {
            Config.ShowInTopBar = parent.DtrEntry.Shown = enabled;
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
