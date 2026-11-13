using KingdomFoundation.Api;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static class ApplySettingsPatch
    {
        internal static void Postfix() => VanillaBindings.ApplySaved();
        internal static void Prefix()
        {
            VanillaBindings.StripInteractions();
            BuildingSettings.Instance?.Commit();
        }
    }
}
