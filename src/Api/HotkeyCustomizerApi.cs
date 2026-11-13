using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer.Api
{
    /// <summary>Opt-in hotkey settings. Call on Unity's main thread after this plugin loads.</summary>
    public static class HotkeyCustomizerApi
    {
        public const string PluginId = HotkeyCustomizerPlugin.Id;
        private static readonly Dictionary<string, ModHotkeySection> sections = new Dictionary<string, ModHotkeySection>(StringComparer.Ordinal);
        private static ModHotkeySection[] snapshot = Array.Empty<ModHotkeySection>();
        private static int mainThread;
        private static bool active;
        internal static void EnsureActive()
        {
            if (!active)
                throw new InvalidOperationException("Hotkey Customizer must load before registering hotkeys. Declare a BepInDependency on " + PluginId + ".");
            if (Environment.CurrentManagedThreadId != mainThread)
                throw new InvalidOperationException("Register and edit hotkeys on Unity's main thread.");
        }

        internal static void Initialize()
        {
            mainThread = Environment.CurrentManagedThreadId;
            active = true;
        }

        /// <summary>Create a mod's Hotkeys controls in its Foundation settings section. Existing controls and config values are preserved.</summary>
        /// <example><code>
        /// // Add a BepInDependency on HotkeyCustomizerApi.PluginId, version 1.5.0.
        /// // In Awake; keep the returned token and dispose it in OnDestroy:
        /// hotkeys = HotkeyCustomizerApi.Register(Info.Metadata.GUID, Info.Metadata.Name, Config);
        /// hotkeys.Add("TogglePanel", "Toggle panel", TogglePanel);
        /// // Or bind an existing ConfigEntry&lt;string&gt;; its saved value is kept:
        /// hotkeys.Add(existingBinding, "Existing action", HandleAction, () =&gt; IsUnlocked);
        /// </code></example>
        public static ModHotkeySection Register(string modId, string modName, ConfigFile config)
        {
            EnsureActive();
            if (string.IsNullOrWhiteSpace(modId) || modId != modId.Trim())
                throw new ArgumentException("Provide a stable mod ID.", nameof(modId));
            if (string.IsNullOrWhiteSpace(modName))
                throw new ArgumentException("Provide a mod display name.", nameof(modName));
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (sections.ContainsKey(modId))
                throw new ArgumentException("This mod already has a hotkey registration.", nameof(modId));
            var section = new ModHotkeySection(modId, modName, config);
            sections.Add(modId, section);
            snapshot = sections.Values.ToArray();
            try
            {
                section.Attach();
                return section;
            }
            catch
            {
                section.Dispose();
                throw;
            }
        }

        internal static void Remove(ModHotkeySection section)
        {
            sections.Remove(section.ModId);
            snapshot = sections.Values.ToArray();
        }

        internal static void Shutdown()
        {
            foreach (var section in snapshot)
                section.Dispose();
            active = false;
        }

        internal static void Update()
        {
            var current = snapshot;
            foreach (var section in current)
                section.Refresh();
            var keyboard = Keyboard.current;
            if (keyboard == null || !HotkeyCustomizerPlugin.CanUseGameplayHotkeys())
                return;
            foreach (var section in current)
                section.Dispatch(keyboard);
        }
    }
}
