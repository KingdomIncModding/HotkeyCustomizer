using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class CancelSettingsPatch
    {
        private static void Prefix() => BuildingSettings.Instance?.Cancel();
    }
}
