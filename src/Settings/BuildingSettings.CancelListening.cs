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
        internal void CancelListening()
        {
            if (Listening >= 0 && Listening < KeyButtons.Count && KeyButtons[Listening])
            {
                var label = KeyButtons[Listening].GetComponentInChildren<TMP_Text>();
                if (label)
                {
                    label.text = BuildingShortcut.Parse(PendingKeys[Listening]).Display;
                    label.color = keyColor != default ? keyColor : Color.white;
                }
            }
            if (nativeLabel)
            {
                nativeLabel.text = nativeText;
                nativeLabel.color = nativeColor;
            }
            ListeningAction = null;
            nativeLabel = null;
            Listening = -1;
        }
    }
}
