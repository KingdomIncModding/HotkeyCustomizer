using System;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class VanillaBindings
    {
        internal static void Apply(InputAction action, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var shortcut = BuildingShortcut.Parse(text);
            string path = shortcut.Key == Key.None ? "" : "<Keyboard>/" + Keyboard.current[shortcut.Key].name;
            action.ApplyBindingOverride(0, new InputBinding {
                overridePath = path,
                overrideInteractions = shortcut.Key == Key.None ? "" : Interaction + "(ctrl=" + (shortcut.Ctrl ? 1 : 0)
                    + ",alt=" + (shortcut.Alt ? 1 : 0) + ",shift=" + (shortcut.Shift ? 1 : 0) + ",meta=" + (shortcut.Meta ? 1 : 0) + ")"
            });
        }
    }
}
