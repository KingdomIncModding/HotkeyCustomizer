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
        internal void Listen(int slot)
        {
            CancelListening();
            Listening = slot;
            listenFrame = Time.frameCount;
            if (slot >= 0 && slot < KeyButtons.Count && KeyButtons[slot])
            {
                var label = KeyButtons[slot].GetComponentInChildren<TMP_Text>();
                if (label)
                {
                    keyColor = label.color;
                    label.text = "Press a key...";
                    label.color = Color.green;
                }
            }
        }
    }
}
