using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dalamud.Plugin.SkipCutscene.Extensions;

public static class SeStringExtensions
{
    public static SeStringBuilder Add(this SeStringBuilder sb, SeString seString)
    {
        foreach(var payload in seString.Payloads)
        {
            sb.Add(payload);
        }
        return sb;
    }
}
