using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Player;
using Assets.Source.Util;
using HarmonyLib;

namespace HotkeyCustomizerConsumerSmoke
{
    [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Initialized), MethodType.Getter)]
    internal static class Offline
    {
        private static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
