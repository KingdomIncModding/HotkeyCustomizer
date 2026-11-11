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
        private void BuildRowDropdown(IModConfigContext context, Transform parent, int slot)
        {
            context.Templates.CreateDropdownRow(parent, "", choiceLabels, Math.Max(0, Choices.IndexOf(Pending[slot] ?? "")), index => {
                CancelListening();
                Pending[slot] = Choices[index];
            }, out var dropdown);
            dropdown.name = "BuildingDropdown_" + slot;
            var rect = (RectTransform)dropdown.transform;
            rect.anchorMin = new Vector2(.22f, .04f);
            rect.anchorMax = new Vector2(.68f, .96f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Dropdowns.Add(dropdown);
        }
    }
}
