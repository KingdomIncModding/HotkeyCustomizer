using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Flow;
using Assets.Source.Player;
using Assets.Source.Util;
using HarmonyLib;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using GameInput = Assets.Behaviour.Input.PlayerInput;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class BuildingSettings : MonoBehaviour
    {
        internal void Begin()
        {
            CancelListening();
            var plugin = HotkeyCustomizerPlugin.Instance;
            PendingShowHotbar = plugin != null && plugin.ShowHotbar.Value;
            PendingVanilla.Clear();
            if (GameInput.RebindableKeys != null)
                foreach (var action in GameInput.RebindableKeys)
                    PendingVanilla[action.name] = VanillaBindings.Entry(action).Value;
            for (int slot = 0; slot < Pending.Length; slot++)
            {
                Pending[slot] = plugin != null ? plugin.Buildings[slot].Value : "";
                PendingKeys[slot] = plugin != null ? plugin.Bindings[slot].Value : "";
            }
        }
    }
}
