using KingdomFoundation.Api;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static class CreateHotbarPatch
    {
        internal static void Postfix(UnityEngine.Component __instance)
        {
            __instance.gameObject.AddComponent<BuildingHotbar>().Build(__instance);
        }
    }
}
