using KingdomFoundation.Api;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static class CancelSettingsPatch
    {
        internal static void Prefix() => BuildingSettings.Instance?.Cancel();
    }
}
