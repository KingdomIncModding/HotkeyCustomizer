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
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class BuildingSettings : MonoBehaviour
    {
        private void Update()
        {
            if ((Listening < 0 && ListeningAction == null) || Time.frameCount == listenFrame || Keyboard.current == null) return;
            var keyboard = Keyboard.current;
            foreach (var key in keyboard.allKeys)
            {
                if (!key.wasPressedThisFrame || BuildingShortcut.Modifier(key.keyCode)) continue;
                if (key.keyCode == Key.Escape) { CancelListening(); return; }
                string value = key.keyCode == Key.Backspace || key.keyCode == Key.Delete
                    ? "" : BuildingShortcut.Capture(keyboard, key.keyCode).ToString();
                if (ListeningAction != null)
                {
                    var action = ListeningAction;
                    var label = nativeLabel;
                    PendingVanilla[action.name] = value == "" ? VanillaBindings.Unassigned : value;
                    VanillaBindings.Apply(action, PendingVanilla[action.name]);
                    CancelListening();
                    label.text = BuildingShortcut.Parse(value).Display;
                    return;
                }
                PendingKeys[Listening] = value;
                CancelListening();
                return;
            }
        }
    }
}
