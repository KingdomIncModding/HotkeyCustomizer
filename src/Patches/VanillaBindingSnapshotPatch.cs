using KingdomFoundation.Api;
using System;
using System.Linq;
using BepInEx.Configuration;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal static class VanillaBindingSnapshotPatch
    {
        internal static void Postfix(ref string ____currentSettings) => ____currentSettings = VanillaBindings.PortableSnapshot();
    }
}
