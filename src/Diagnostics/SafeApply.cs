using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Player;
using Assets.Source.Util;
using HarmonyLib;

namespace HotkeyCustomizerConsumerSmoke
{
    [HarmonyPatch(typeof(OptionsUI), nameof(OptionsUI.ApplyAndClose))]
    internal static class SafeApply
    {
        [HarmonyPriority(Priority.Last)]
        private static bool Prefix()
        {
            // Foundation's normal prefix commits all sections first. Skip only
            // the game's display/registry writes and return to the menu.
            MainMenuUI.Instance.ShowMainMenu();
            return false;
        }
    }
}
