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
        internal bool SelectBuilding(int slot)
        {
            if (slot < 0 || slot >= Buildings.Length || !CanUseBuildings()) return false;
            string id = Buildings[slot].Value;
            if (string.IsNullOrEmpty(id)) return false;
            var player = FoundationApi.World.Player;
            var element = FoundationApi.Machines.Catalog.FirstOrDefault(e => e.ElementType == id && e.RequiredTech != null);
            if (element == null || !element.IsPurchaseable(player)) return false;
            if (!player.CanAfford(element.GetPurchaseCost()))
            {
                UISounds.CraftStep();
                GameUI.Instance.InsufficientFundsWarning();
                return false;
            }
            // This is the game's preview; a shortcut never purchases a machine itself.
            WorldFlowManager.Instance.StopBlueprintMode();
            WorldFlowManager.Instance.ShowPurchaseBlueprint(element, false);
            return true;
        }
    }
}
