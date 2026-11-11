using System;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    [HarmonyPatch(typeof(KeyBindingRow), "_resetBindingText")]
    internal static partial class VanillaBindingLabelPatch
    {

    }
}
