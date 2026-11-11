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
        private void BuildRow(IModConfigContext context, int slot)
        {
            var row = context.Templates.CreateEmptyRow(context.Container, 54f);
            row.name = "BuildingRow_" + slot;
            var labelObject = new GameObject("SlotLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(row.transform, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = new Vector2(.20f, 1);
            labelRect.offsetMin = new Vector2(8, 0);
            labelRect.offsetMax = Vector2.zero;
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.fontSize = 16;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12;
            label.fontSizeMax = 18;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.text = "Building " + slot;
            BuildRowDropdown(context, row.transform, slot);
            BuildRowKeyButton(context, row.transform, slot);
        }
    }
}
