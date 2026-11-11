using System;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Player;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class HotkeyCustomizerPlugin : BaseUnityPlugin
    {
        internal IModConfigSection SettingsSection() => foundation.Settings(
            "Hotkey Customizer", "Configure building shortcuts (slots 0-9) and the building hotbar.");
    }
}
