using System;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    [HarmonyPatch(typeof(KeyBindingsUI), "OnEnable")]
    internal static partial class VanillaBindingSnapshotPatch
    {

    }
}
