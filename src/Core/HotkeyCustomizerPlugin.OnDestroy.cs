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
        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            foundation?.Dispose();
            if (Instance == this) Instance = null;
        }
    }
}
