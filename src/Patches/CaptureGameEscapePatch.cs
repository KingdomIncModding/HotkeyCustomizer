using KingdomFoundation.Api;

namespace KingdomHotkeyCustomizer
{

    internal static class CaptureGameEscapePatch
    {
        internal static bool Prefix() => !ModHotkeyCapture.BlocksEscape;
    }
}
