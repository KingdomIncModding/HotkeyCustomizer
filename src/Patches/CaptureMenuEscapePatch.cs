using KingdomFoundation.Api;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{

    internal static class CaptureMenuEscapePatch
    {
        internal static bool Prefix() => Keyboard.current?.escapeKey.wasPressedThisFrame != true || !ModHotkeyCapture.BlocksEscape;
    }
}
