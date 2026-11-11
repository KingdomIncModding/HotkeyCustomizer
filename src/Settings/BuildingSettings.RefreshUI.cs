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
        internal void RefreshUI()
        {
            if (HotbarToggle) HotbarToggle.SetIsOnWithoutNotify(PendingShowHotbar);
            for (int i = 0; i < Pending.Length; i++)
            {
                if (i < Dropdowns.Count && Dropdowns[i] != null && Choices != null)
                {
                    int index = Math.Max(0, Choices.IndexOf(Pending[i] ?? ""));
                    Dropdowns[i].SetValueWithoutNotify(index);
                    Dropdowns[i].RefreshShownValue();
                }
                if (i < KeyButtons.Count && KeyButtons[i] != null)
                {
                    var label = KeyButtons[i].GetComponentInChildren<TMP_Text>();
                    if (label) label.text = BuildingShortcut.Parse(PendingKeys[i]).Display;
                }
            }
        }
    }
}
