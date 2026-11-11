using System;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class VanillaBindingLabelPatch
    {
        private static void Postfix(KeyBindingRow __instance, InputAction ____bindingRef, TMP_Text ____binding)
        {
            if (____bindingRef == null) return;
            var settings = __instance.GetComponentInParent<BuildingSettings>() ?? BuildingSettings.Instance;
            string value = settings != null && settings.PendingVanilla.TryGetValue(____bindingRef.name, out var pending)
                ? pending : VanillaBindings.Entry(____bindingRef).Value;
            if (!string.IsNullOrEmpty(value)) ____binding.text = BuildingShortcut.Parse(value).Display;
        }
    }
}
