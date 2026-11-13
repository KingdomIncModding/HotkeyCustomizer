using KingdomFoundation.Api;
using System;
using System.Linq;
using BepInEx.Configuration;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static class VanillaBindingLoadPatch
    {
        internal static void Postfix() => VanillaBindings.ApplySaved();
    }
}
