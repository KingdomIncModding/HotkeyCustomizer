using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using HarmonyLib;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class CreateHotbarPatch
    {
        private static void Postfix(GameUI __instance)
        {
            __instance.gameObject.AddComponent<BuildingHotbar>().Build(__instance);
        }
    }
}
