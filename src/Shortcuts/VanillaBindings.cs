using KingdomFoundation.Api;
using System;
using System.Linq;
using BepInEx.Configuration;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static class VanillaBindings
    {
        internal static void Apply(InputAction action, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            var shortcut = BuildingShortcut.Parse(text);
            string path = shortcut.Key == Key.None ? "" : "<Keyboard>/" + Keyboard.current[shortcut.Key].name;
            action.ApplyBindingOverride(0, new InputBinding { overridePath = path, overrideInteractions = shortcut.Key == Key.None ? "" : Interaction + "(ctrl=" + (shortcut.Ctrl ? 1 : 0) + ",alt=" + (shortcut.Alt ? 1 : 0) + ",shift=" + (shortcut.Shift ? 1 : 0) + ",meta=" + (shortcut.Meta ? 1 : 0) + ")" });
        }

        internal static void ApplySaved()
        {
            if (FoundationApi.Bindings.Rebindable == null)
                return;
            foreach (var action in FoundationApi.Bindings.Rebindable)
                Apply(action, Entry(action).Value);
        }

        internal const string Interaction = "KingdomHotkeyModifiers";
        internal const string Unassigned = "Unassigned";
        internal static ConfigEntry<string> Entry(InputAction action) => HotkeyCustomizerPlugin.Instance.Config.Bind("VanillaHotkeys", action.name, "", "Optional user-selected shortcut. Empty preserves the game's binding; Unassigned clears it.");
        internal static string PortableSnapshot()
        {
            string current = FoundationApi.Bindings.Actions.SaveBindingOverridesAsJson();
            try
            {
                StripInteractions();
                return FoundationApi.Bindings.Actions.SaveBindingOverridesAsJson();
            }
            finally
            {
                FoundationApi.Bindings.Actions.LoadBindingOverridesFromJson(current);
            }
        }

        internal static void StripInteractions()
        {
            if (FoundationApi.Bindings.Rebindable == null)
                return;
            foreach (var action in FoundationApi.Bindings.Rebindable)
            {
                var binding = action.bindings[0];
                if (binding.overrideInteractions?.StartsWith(Interaction + "(", StringComparison.Ordinal) == true)
                    action.ApplyBindingOverride(0, new InputBinding { overridePath = binding.overridePath, overrideInteractions = "" });
            }
        }
    }
}
