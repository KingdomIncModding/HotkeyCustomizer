using System;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace KingdomHotkeyCustomizer
{
    internal static partial class VanillaBindings
    {
        internal static void StripInteractions()
        {
            if (GameInput.RebindableKeys == null) return;
            foreach (var action in GameInput.RebindableKeys)
            {
                var binding = action.bindings[0];
                if (binding.overrideInteractions?.StartsWith(Interaction + "(", StringComparison.Ordinal) == true)
                    action.ApplyBindingOverride(0, new InputBinding { overridePath = binding.overridePath, overrideInteractions = "" });
            }
        }
    }
}
