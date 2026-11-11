using System;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class VanillaBindingLoadPatch
    {
        private static void Postfix() => VanillaBindings.ApplySaved();
    }
}
