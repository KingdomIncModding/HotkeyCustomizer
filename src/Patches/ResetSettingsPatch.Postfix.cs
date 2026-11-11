using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class ResetSettingsPatch
    {
        private static void Postfix() => BuildingSettings.Instance?.PendingVanilla.Clear();
    }
}
