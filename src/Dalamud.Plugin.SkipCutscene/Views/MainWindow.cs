using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Dalamud.Plugin.SkipCutscene.Views;

internal class MainWindow(Config config) : Window($"{nameof(SkipCutscene)} main window"), IDisposable
{
    public override void Draw()
    {
        ImGui.Text($"Skipping enabled: {config.IsEnabled}");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
