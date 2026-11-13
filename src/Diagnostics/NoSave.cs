using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Player;
using Assets.Source.Util;
using HarmonyLib;

namespace HotkeyCustomizerConsumerSmoke
{
    [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.Store))]
    internal static class NoSave
    {
        private static bool Prefix() => false;
    }
}
