using KingdomFoundation.Api;
using System;
using System.Linq;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer.Api
{
    public sealed class ModHotkeySection : IDisposable
    {
        /// <summary>Bind a string entry in this mod's Hotkeys config section. Blank defaults are unassigned. Callbacks run during gameplay only.</summary>
        public ModHotkey Add(string id, string label, Action onPressed = null, string defaultBinding = "", Func<bool> canExecute = null)
        {
            HotkeyCustomizerApi.EnsureActive();
            if (!IsRegistered)
                throw new ObjectDisposedException(nameof(ModHotkeySection));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Provide a hotkey label.", nameof(label));
            var definition = new ConfigDefinition("Hotkeys", id);
            if (hotkeys.Any(key => key.Entry.Definition.Equals(definition)))
                throw new ArgumentException("This hotkey is already registered.", nameof(id));
            if (!string.IsNullOrWhiteSpace(defaultBinding) && BuildingShortcut.Parse(defaultBinding).Key == UnityEngine.InputSystem.Key.None)
                throw new ArgumentException("Use a key name with optional Ctrl, Alt, Shift and Meta modifiers.", nameof(defaultBinding));
            return Add(Config.Bind(definition, defaultBinding ?? "", new ConfigDescription(label)), label, onPressed, canExecute);
        }

        /// <summary>Use an existing string config entry without changing its saved value or default. A null callback provides settings-only integration.</summary>
        public ModHotkey Add(ConfigEntry<string> binding, string label, Action onPressed = null, Func<bool> canExecute = null)
        {
            HotkeyCustomizerApi.EnsureActive();
            if (!IsRegistered)
                throw new ObjectDisposedException(nameof(ModHotkeySection));
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));
            if (!ReferenceEquals(binding.ConfigFile, Config))
                throw new ArgumentException("The entry must belong to this mod's configuration file.", nameof(binding));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Provide a hotkey label.", nameof(label));
            if (hotkeys.Any(key => key.Entry.Definition.Equals(binding.Definition)))
                throw new ArgumentException("This hotkey is already registered.", nameof(binding));
            var key = new ModHotkey(this, binding, label, onPressed, canExecute);
            hotkeys.Add(key);
            snapshot = hotkeys.ToArray();
            section.RemoveElement(element);
            section.AddElement(element);
            return key;
        }

        internal void Attach()
        {
            createdSection = !FoundationApi.ModConfig.TryGetSection(ModId, out section);
            if (createdSection)
                section = FoundationApi.ModConfig.GetOrCreateSection(ModId, ModName);
            element = new ModHotkeyConfigElement(this);
            section.AddElement(element);
        }

        internal void BuildView(IModConfigContext context)
        {
            Refresh();
            context.Templates.CreateTextRow(context.Container, "Hotkeys", true, out _);
            foreach (var key in snapshot)
                key.BuildView(context);
            context.Templates.CreateButtonRow(context.Container, "Defaults", "Reset Hotkeys", Reset, out _, out _);
        }

        internal void Cancel()
        {
            ModHotkeyCapture.Instance?.CancelFor(this);
            foreach (var key in snapshot)
            {
                key.PendingValue = key.Entry.Value ?? "";
                key.Refresh();
            }
        }

        internal void Commit()
        {
            if (!IsRegistered)
                return;
            ModHotkeyCapture.Instance?.CancelFor(this);
            bool saveOnSet = Config.SaveOnConfigSet;
            bool changed = false;
            Config.SaveOnConfigSet = false;
            try
            {
                foreach (var key in snapshot)
                {
                    key.Refresh();
                    if (key.PendingValue == key.Entry.Value)
                        continue;
                    key.Entry.Value = key.PendingValue;
                    changed = true;
                    key.PendingValue = key.Entry.Value ?? "";
                    key.Refresh();
                }

                if (changed)
                    Config.Save();
            }
            finally
            {
                Config.SaveOnConfigSet = saveOnSet;
            }
        }

        internal ModHotkeySection(string modId, string modName, ConfigFile config)
        {
            ModId = modId;
            ModName = modName;
            Config = config;
        }

        public string ModId { get; }
        public string ModName { get; }
        public ConfigFile Config { get; }
        public bool IsRegistered { get; private set; } = true;

        private readonly List<ModHotkey> hotkeys = new List<ModHotkey>();
        private ModHotkey[] snapshot = Array.Empty<ModHotkey>();
        private IModConfigSection section;
        private ModHotkeyConfigElement element;
        private bool createdSection;
        private int lastDispatchFrame = -1;
        internal void Dispatch(Keyboard keyboard)
        {
            if (!IsRegistered || lastDispatchFrame == Time.frameCount)
                return;
            lastDispatchFrame = Time.frameCount;
            foreach (var key in snapshot)
            {
                if (!IsRegistered)
                    break;
                if (!key.IsRegistered || key.OnPressed == null || !key.Shortcut.Pressed(keyboard))
                    continue;
                try
                {
                    if (key.CanExecute != null && !key.CanExecute())
                        continue;
                    key.OnPressed();
                }
                catch (Exception error)
                {
                    HotkeyCustomizerPlugin.Instance?.ReportHotkeyError(ModId + " hotkey " + key.Id + ": " + error);
                }

                break;
            }
        }

        public void Dispose()
        {
            if (!IsRegistered)
                return;
            HotkeyCustomizerApi.EnsureActive();
            IsRegistered = false;
            ModHotkeyCapture.Instance?.CancelFor(this);
            foreach (var key in snapshot)
                key.Dispose();
            section?.RemoveElement(element);
            if (createdSection && section?.Elements.Count == 0 && FoundationApi.ModConfig.TryGetSection(ModId, out var current) && ReferenceEquals(current, section))
                FoundationApi.ModConfig.RemoveSection(ModId);
            HotkeyCustomizerApi.Remove(this);
        }

        internal void Refresh()
        {
            if (!IsRegistered)
                return;
            foreach (var key in snapshot)
                key.Refresh();
        }

        internal void Remove(ModHotkey key)
        {
            hotkeys.Remove(key);
            snapshot = hotkeys.ToArray();
            if (IsRegistered && section != null)
            {
                section.RemoveElement(element);
                section.AddElement(element);
            }
        }

        internal void Reset()
        {
            ModHotkeyCapture.Instance?.CancelFor(this);
            foreach (var key in snapshot)
            {
                key.PendingValue = key.Entry.DefaultValue as string ?? "";
                key.Refresh();
            }
        }
    }
}
