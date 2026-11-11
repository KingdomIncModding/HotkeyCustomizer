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
        internal void ResetChoices()
        {
            CancelListening();
            PendingShowHotbar = true;
            for (int slot = 0; slot < Pending.Length; slot++)
            {
                Pending[slot] = "";
                PendingKeys[slot] = "";
            }
            foreach (var name in PendingVanilla.Keys.ToArray()) PendingVanilla[name] = "";
            RefreshUI();
        }
    }
}
