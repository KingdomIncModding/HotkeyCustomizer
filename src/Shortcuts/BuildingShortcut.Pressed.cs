using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal partial struct BuildingShortcut
    {
        internal bool Pressed(Keyboard keyboard) => Key != Key.None && keyboard[Key].wasPressedThisFrame
            && Ctrl == keyboard.ctrlKey.isPressed && Alt == keyboard.altKey.isPressed
            && Shift == keyboard.shiftKey.isPressed
            && Meta == (keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed);
    }
}
