using System;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class VanillaBindings
    {
        internal static ConfigEntry<string> Entry(InputAction action) => HotkeyCustomizerPlugin.Instance.Config.Bind(
            "VanillaHotkeys", action.name, "", "Optional user-selected shortcut. Empty preserves the game's binding; Unassigned clears it.");
    }
}
