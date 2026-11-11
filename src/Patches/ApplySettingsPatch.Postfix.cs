using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class ApplySettingsPatch
    {
        private static void Postfix() => VanillaBindings.ApplySaved();
    }
}
