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
    // Keep the native actions, callbacks and binding IDs. The interaction gates
    // their performed callbacks on the exact user-selected modifier state.
    public sealed class ShortcutModifiers : IInputInteraction
    {
        public int ctrl, alt, shift, meta;
        public void Process(ref InputInteractionContext context)
        {
            var keyboard = context.control.device as Keyboard ?? Keyboard.current;
            bool matches = keyboard != null && (ctrl != 0) == keyboard.ctrlKey.isPressed
                && (alt != 0) == keyboard.altKey.isPressed && (shift != 0) == keyboard.shiftKey.isPressed
                && (meta != 0) == (keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed);
            if (matches && context.ControlIsActuated())
            {
                if (context.phase == InputActionPhase.Waiting) context.Started();
                if (context.phase == InputActionPhase.Started) context.PerformedAndStayPerformed();
            }
            else if (context.phase != InputActionPhase.Waiting) context.Canceled();
        }
        public void Reset() { }
    }

    internal static class VanillaBindings
    {
        internal const string Interaction = "KingdomHotkeyModifiers";
        internal const string Unassigned = "Unassigned";
        internal static ConfigEntry<string> Entry(InputAction action) => HotkeyCustomizerPlugin.Instance.Config.Bind(
            "VanillaHotkeys", action.name, "", "Optional user-selected shortcut. Empty preserves the game's binding; Unassigned clears it.");

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

        internal static void ApplySaved()
        {
            if (GameInput.RebindableKeys == null) return;
            foreach (var action in GameInput.RebindableKeys) Apply(action, Entry(action).Value);
        }

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

        internal static string PortableSnapshot()
        {
            string current = GameInput.Asset.SaveBindingOverridesAsJson();
            try { StripInteractions(); return GameInput.Asset.SaveBindingOverridesAsJson(); }
            finally { GameInput.Asset.LoadBindingOverridesFromJson(current); }
        }

        [HarmonyPatch(typeof(KeyBindingsUI), nameof(KeyBindingsUI.Apply))]
        internal static class Load { static void Postfix() => ApplySaved(); }

        [HarmonyPatch(typeof(KeyBindingsUI), "OnEnable")]
        internal static class Snapshot
        {
            static void Postfix(ref string ____currentSettings) => ____currentSettings = PortableSnapshot();
        }

        [HarmonyPatch(typeof(KeyBindingRow), "_resetBindingText")]
        internal static class Label
        {
            static void Postfix(KeyBindingRow __instance, InputAction ____bindingRef, TMP_Text ____binding)
            {
                if (____bindingRef == null) return;
                var settings = __instance.GetComponentInParent<BuildingSettings>() ?? BuildingSettings.Instance;
                string value = settings != null && settings.PendingVanilla.TryGetValue(____bindingRef.name, out var pending)
                    ? pending : Entry(____bindingRef).Value;
                if (!string.IsNullOrEmpty(value)) ____binding.text = BuildingShortcut.Parse(value).Display;
            }
        }
    }
}
