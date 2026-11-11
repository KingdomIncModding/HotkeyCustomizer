using System;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static partial class VanillaBindingSnapshotPatch
    {
        private static void Postfix(ref string ____currentSettings) => ____currentSettings = VanillaBindings.PortableSnapshot();
    }
}
