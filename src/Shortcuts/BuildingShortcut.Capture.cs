using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal partial struct BuildingShortcut
    {
        internal static BuildingShortcut Capture(Keyboard keyboard, Key key) => new BuildingShortcut {
            Key = key, Ctrl = keyboard.ctrlKey.isPressed, Alt = keyboard.altKey.isPressed,
            Shift = keyboard.shiftKey.isPressed,
            Meta = keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed
        };
    }
}
