using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Player;
using Assets.Source.Util;
using HarmonyLib;

namespace HotkeyCustomizerConsumerSmoke
{
    [HarmonyPatch(typeof(SteamAchievement), nameof(SteamAchievement.Trigger))]
    internal static class NoAchievement
    {
        private static bool Prefix() => false;
    }
}
