using KingdomFoundation.Api;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using BepInEx.Configuration;

namespace KingdomHotkeyCustomizer.Api
{
    public sealed class ModHotkey : IDisposable
    {
        internal void BuildView(IModConfigContext context)
        {
            context.Templates.CreateButtonRow(context.Container, Label, BuildingShortcut.Parse(PendingValue).Display, () => ModHotkeyCapture.Instance?.Listen(this, ActiveLabel), out var button, out var label);
            button.name = "ModHotkey_" + Entry.Definition.Section + "_" + Entry.Definition.Key;
            button.navigation = new Navigation
            {
                mode = Navigation.Mode.None
            };
            button.transition = Selectable.Transition.None;
            ActiveLabel = label;
            var image = button.targetGraphic as Image;
            var rows = FoundationApi.Bindings.Rows.ToArray();
            if (rows.Length != 0)
            {
                var nativeText = FoundationApi.Bindings.Text(rows[0]);
                var nativeImage = FoundationApi.Bindings.Button(rows[0]);
                if (label && nativeText)
                {
                    label.font = nativeText.font;
                    label.color = nativeText.color;
                }

                if (image && nativeImage)
                {
                    image.sprite = nativeImage.sprite;
                    image.type = nativeImage.type;
                }
            }

            if (image)
            {
                image.color = new Color(0, 0, 0, .5f);
                button.gameObject.AddComponent<NativeKeyHover>().Target = image;
            }
        }

        internal ModHotkey(ModHotkeySection owner, ConfigEntry<string> entry, string label, Action onPressed, Func<bool> canExecute)
        {
            Owner = owner;
            Entry = entry;
            Label = label;
            OnPressed = onPressed;
            CanExecute = canExecute;
            savedValue = PendingValue = entry.Value ?? "";
            Shortcut = BuildingShortcut.Parse(savedValue);
        }

        public string Id => Entry.Definition.ToString();
        public string Label { get; }
        public ConfigEntry<string> Entry { get; }
        public string Binding => Entry.Value;
        public bool IsRegistered { get; private set; } = true;
        internal ModHotkeySection Owner { get; }

        internal Action OnPressed;
        internal Func<bool> CanExecute;
        internal BuildingShortcut Shortcut;
        internal string PendingValue;
        private string savedValue;
        internal TMP_Text ActiveLabel;
        public void Dispose()
        {
            if (!IsRegistered)
                return;
            HotkeyCustomizerApi.EnsureActive();
            if (ModHotkeyCapture.Instance?.Current == this)
                ModHotkeyCapture.Instance.Cancel();
            IsRegistered = false;
            OnPressed = null;
            CanExecute = null;
            Owner.Remove(this);
            ActiveLabel = null;
        }

        internal void Refresh()
        {
            string current = Entry.Value ?? "";
            if (current != savedValue)
            {
                if (PendingValue == savedValue)
                    PendingValue = current;
                savedValue = current;
                Shortcut = BuildingShortcut.Parse(current);
            }

            if (ActiveLabel && ModHotkeyCapture.Instance?.Current != this)
                ActiveLabel.text = BuildingShortcut.Parse(PendingValue).Display;
        }
    }
}
