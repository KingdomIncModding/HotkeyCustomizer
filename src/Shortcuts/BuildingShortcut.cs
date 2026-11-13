using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal struct BuildingShortcut
    {
        internal static BuildingShortcut Capture(Keyboard keyboard, Key key) => new BuildingShortcut
        {
            Key = key,
            Ctrl = keyboard.ctrlKey.isPressed,
            Alt = keyboard.altKey.isPressed,
            Shift = keyboard.shiftKey.isPressed,
            Meta = keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed
        };
        internal Key Key;
        internal bool Ctrl, Alt, Shift, Meta;
        internal string Display => Key == Key.None ? "Unassigned" : ToString().Replace("Digit", "").Replace("Numpad", "Num ").Replace("+", " + ");

        internal static bool Modifier(Key key) => key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftAlt || key == Key.RightAlt || key == Key.LeftShift || key == Key.RightShift || key == Key.LeftMeta || key == Key.RightMeta;
        internal static BuildingShortcut Parse(string text)
        {
            var result = new BuildingShortcut();
            if (string.IsNullOrWhiteSpace(text))
                return result;
            var parts = text.Split('+');
            if (!Enum.TryParse(parts[parts.Length - 1], out result.Key) || !Enum.IsDefined(typeof(Key), result.Key) || result.Key == Key.None || Modifier(result.Key))
                return default;
            for (int i = 0; i < parts.Length - 1; i++)
                switch (parts[i])
                {
                    case "Ctrl":
                        result.Ctrl = true;
                        break;
                    case "Alt":
                        result.Alt = true;
                        break;
                    case "Shift":
                        result.Shift = true;
                        break;
                    case "Meta":
                        result.Meta = true;
                        break;
                    default:
                        return default;
                }

            return result;
        }

        internal bool Pressed(Keyboard keyboard) => Key != Key.None && keyboard[Key].wasPressedThisFrame && Ctrl == keyboard.ctrlKey.isPressed && Alt == keyboard.altKey.isPressed && Shift == keyboard.shiftKey.isPressed && Meta == (keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed);
        public override string ToString()
        {
            if (Key == Key.None)
                return "";
            var parts = new List<string>();
            if (Ctrl)
                parts.Add("Ctrl");
            if (Alt)
                parts.Add("Alt");
            if (Shift)
                parts.Add("Shift");
            if (Meta)
                parts.Add("Meta");
            parts.Add(Key.ToString());
            return string.Join("+", parts);
        }
    }
}
