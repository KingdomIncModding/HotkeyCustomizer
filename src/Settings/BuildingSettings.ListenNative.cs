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
        internal void ListenNative(InputAction action, TMP_Text label)
        {
            CancelListening();
            ListeningAction = action;
            nativeLabel = label;
            nativeText = label.text;
            nativeColor = label.color;
            listenFrame = Time.frameCount;
            label.text = "Press a key...";
            label.color = Color.green;
        }
    }
}
