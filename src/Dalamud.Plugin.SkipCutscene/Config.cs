using Dalamud.Configuration;

namespace Dalamud.Plugin.SkipCutscene;

public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool IsEnabled { get; set; } = true;

    public bool AutoSkipOnLightParty { get; set; } = true;

    public bool ShowInTopBar { get; set; } = true;
}
