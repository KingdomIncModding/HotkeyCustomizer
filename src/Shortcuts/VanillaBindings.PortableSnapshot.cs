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
        internal static string PortableSnapshot()
        {
            string current = GameInput.Asset.SaveBindingOverridesAsJson();
            try { StripInteractions(); return GameInput.Asset.SaveBindingOverridesAsJson(); }
            finally { GameInput.Asset.LoadBindingOverridesFromJson(current); }
        }
    }
}
