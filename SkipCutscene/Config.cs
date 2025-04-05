using System.ComponentModel;
using Dalamud.Configuration;

namespace Dalamud.Plugin.SkipCutscene
{
    public class Config : IPluginConfiguration
    {

        public int Version { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
