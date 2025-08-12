using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Common.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Plugin.SkipCutscene.GameData;
public static class Hooks
{
    private const string CutsceneCreationSignature = "48 89 5C 24 ?? 57 48 83 EC ?? 48 8B D1 48 8D 4C 24 ?? E8 ?? ?? ?? ?? 48 8B 4C 24 ?? BA ?? ?? ?? ?? B3 ?? E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8D 4C 24 ?? 48 8B F8 E8 ?? ?? ?? ?? 48 8B 4C 24 ?? 4C 8B C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? ?? ?? ?? 84 99";
    // Todo, figure out what data is being pointed to.
    public delegate bool CutsceneCreation(IntPtr pointer);
    public static Hook<CutsceneCreation> CreateCutsceneCreationHook(
        IGameInteropProvider interopProvider,
        CutsceneCreation @delegate
    ) =>
        interopProvider.HookFromSignature(CutsceneCreationSignature, @delegate);

    // Currently not used, may be helpful if we want to filter cutscenes.
    public static Hook<UIState.Delegates.IsCutsceneSeen> CreateCutsceneHook(
        IGameInteropProvider interopProvider,
        UIState.Delegates.IsCutsceneSeen @delegate
    ) =>
        interopProvider.HookFromSignature(UIState.Addresses.IsCutsceneSeen.String, @delegate);

}
