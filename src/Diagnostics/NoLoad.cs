using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Player;
using Assets.Source.Util;
using HarmonyLib;

namespace HotkeyCustomizerConsumerSmoke
{
    [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.LoadLatestSave))]
    internal static class NoLoad
    {
        private static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
