using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal partial struct BuildingShortcut
    {
        internal static bool Modifier(Key key) => key == Key.LeftCtrl || key == Key.RightCtrl
            || key == Key.LeftAlt || key == Key.RightAlt || key == Key.LeftShift || key == Key.RightShift
            || key == Key.LeftMeta || key == Key.RightMeta;
    }
}
