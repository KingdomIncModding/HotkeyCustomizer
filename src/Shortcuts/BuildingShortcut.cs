using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal partial struct BuildingShortcut
    {
        internal Key Key;
        internal bool Ctrl, Alt, Shift, Meta;
        internal string Display => Key == Key.None ? "Unassigned" : ToString().Replace("Digit", "").Replace("Numpad", "Num ").Replace("+", " + ");
    }
}
