using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class NativeRebindPatch
    {
        private static bool Prefix(PointerEventData eventData, InputAction ____bindingRef, TMP_Text ____binding)
        {
            if (eventData.button == PointerEventData.InputButton.Left && BuildingSettings.Instance != null)
                BuildingSettings.Instance.ListenNative(____bindingRef, ____binding);
            return false;
        }
    }
}
