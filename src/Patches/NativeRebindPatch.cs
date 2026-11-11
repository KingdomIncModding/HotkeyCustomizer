using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    [HarmonyPatch(typeof(KeyBindingRow), nameof(KeyBindingRow.OnPointerClick))]
    internal static partial class NativeRebindPatch
    {

    }
}
