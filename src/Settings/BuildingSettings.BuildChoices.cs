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
        private void BuildChoices()
        {
            var elements = BuildingCatalog.All().OrderBy(e => Translation.Translate(e.Label), StringComparer.CurrentCultureIgnoreCase).ToList();
            Choices = new List<string> { "" };
            Choices.AddRange(elements.Select(e => e.ElementType));
            Choices.AddRange(Pending.Where(id => !string.IsNullOrEmpty(id) && !Choices.Contains(id)).Distinct());
            // Native IDs and the related display labels must have matching indexes.
            choiceLabels = new List<string> { "Unassigned" };
            choiceLabels.AddRange(elements.Select(e => Translation.Translate(e.Label)
                + (FoundationApi.World.Player != null && !e.IsPurchaseable(FoundationApi.World.Player) ? " (locked)" : "")));
            choiceLabels.AddRange(Choices.Skip(elements.Count + 1).Select(id => id + " (unavailable)"));
        }
    }
}
