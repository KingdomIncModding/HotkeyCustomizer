using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    [HarmonyPatch(typeof(KeyBindingsUI), nameof(KeyBindingsUI.CancelBindings))]
    internal static partial class CancelSettingsPatch
    {

    }
}
