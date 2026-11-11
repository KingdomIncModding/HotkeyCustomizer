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
        internal void Commit()
        {
            var plugin = HotkeyCustomizerPlugin.Instance;
            if (plugin == null) return;
            bool saveOnSet = plugin.Config.SaveOnConfigSet;
            plugin.Config.SaveOnConfigSet = false;
            CancelListening();
            try
            {
                for (int slot = 0; slot < Pending.Length; slot++)
                {
                    plugin.Buildings[slot].Value = Pending[slot] ?? "";
                    plugin.Bindings[slot].Value = PendingKeys[slot] ?? "";
                }
                foreach (var name in PendingVanilla.Keys.ToArray())
                {
                    var action = GameInput.Asset?.FindAction(name, false);
                    if (action != null) VanillaBindings.Entry(action).Value = PendingVanilla[name];
                }
                plugin.ShowHotbar.Value = PendingShowHotbar;
            }
            finally { plugin.Config.SaveOnConfigSet = saveOnSet; }
            plugin.Config.Save();
        }
    }
}
