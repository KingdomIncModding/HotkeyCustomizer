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
        internal static bool CanUseBuildings()
        {
            if (!Unlocked || !GameUI.Instance || !FoundationApi.View.GameInputEnabled || Time.timeScale == 0
                || !WorldFlowManager.Visible || UIAlertWindow.Active) return false;
            var selected = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            if (selected && (selected.GetComponentInParent<TMP_InputField>() || selected.GetComponentInParent<TMP_Dropdown>())) return false;
            if (GameUI.Instance.IsActive(GameUIType.Popup) || GameUI.Instance.IsActive(GameUIType.Tech)
                || GameUI.Instance.IsActive(GameUIType.Clone) || GameUI.Instance.IsActive(GameUIType.AreaMove)) return false;
            return !UnityEngine.Object.FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).Any(f => f.isFocused);
        }
    }
}
