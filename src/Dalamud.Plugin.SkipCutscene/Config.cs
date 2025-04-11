using Dalamud.Configuration;

namespace Dalamud.Plugin.SkipCutscene;

public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool IsEnabled { get; set; } = true;

    public string Command { get ; set; } = "sc";
}
