using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using KingdomFoundation.Api;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static class VanillaBindings
    {
        private static readonly Dictionary<InputAction, (string Path, string Interactions)> originalOverrides = new Dictionary<InputAction, (string Path, string Interactions)>();

        internal static bool PendingApply { get; private set; }

        internal static void Apply(InputAction action, string text)
        {
            if (action == null || string.IsNullOrEmpty(text))
                return;

            var shortcut = BuildingShortcut.Parse(text);
            string path;
            if (shortcut.Key == Key.None)
            {
                path = "";
            }
            else
            {
                var keyboard = Keyboard.current;
                if (keyboard == null)
                {
                    PendingApply = true;
                    return;
                }

                var keyControl = keyboard[shortcut.Key];
                if (keyControl == null)
                    return;
                path = "<Keyboard>/" + keyControl.name;
            }

            if (!originalOverrides.ContainsKey(action) && action.bindings.Count > 0)
            {
                var current = action.bindings[0];
                originalOverrides[action] = (current.overridePath, current.overrideInteractions);
            }

            action.ApplyBindingOverride(0, new InputBinding { overridePath = path, overrideInteractions = shortcut.Key == Key.None ? "" : Interaction + "(ctrl=" + (shortcut.Ctrl ? 1 : 0) + ",alt=" + (shortcut.Alt ? 1 : 0) + ",shift=" + (shortcut.Shift ? 1 : 0) + ",meta=" + (shortcut.Meta ? 1 : 0) + ")" });
        }

        internal static void ApplySaved()
        {
            PendingApply = false;
            if (FoundationApi.Bindings.Rebindable == null)
                return;
            foreach (var action in FoundationApi.Bindings.Rebindable)
                Apply(action, Entry(action).Value);
        }

        internal static void Restore()
        {
            PendingApply = false;
            foreach (var pair in originalOverrides)
            {
                if (pair.Key != null && pair.Key.bindings.Count > 0)
                    pair.Key.ApplyBindingOverride(0, new InputBinding { overridePath = pair.Value.Path, overrideInteractions = pair.Value.Interactions });
            }

            originalOverrides.Clear();
            StripInteractions();
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
