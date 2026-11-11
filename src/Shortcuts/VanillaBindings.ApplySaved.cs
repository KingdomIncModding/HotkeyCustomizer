using System;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace KingdomHotkeyCustomizer
{
    internal static partial class VanillaBindings
    {
        internal static void ApplySaved()
        {
            if (GameInput.RebindableKeys == null) return;
            foreach (var action in GameInput.RebindableKeys) Apply(action, Entry(action).Value);
        }
    }
}
