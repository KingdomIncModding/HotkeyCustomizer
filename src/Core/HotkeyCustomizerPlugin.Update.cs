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
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            for (int slot = 0; slot < 10; slot++)
                if (Shortcuts[slot].Pressed(keyboard)) { SelectBuilding(slot); break; }
        }
    }
}
