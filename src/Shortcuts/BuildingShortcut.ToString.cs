using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal partial struct BuildingShortcut
    {
        public override string ToString()
        {
            if (Key == Key.None) return "";
            var parts = new List<string>();
            if (Ctrl) parts.Add("Ctrl");
            if (Alt) parts.Add("Alt");
            if (Shift) parts.Add("Shift");
            if (Meta) parts.Add("Meta");
            parts.Add(Key.ToString());
            return string.Join("+", parts);
        }
    }
}
