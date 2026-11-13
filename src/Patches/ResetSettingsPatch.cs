using KingdomFoundation.Api;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static class ResetSettingsPatch
    {
        internal static void Postfix() => BuildingSettings.Instance?.PendingVanilla.Clear();
    }
}
