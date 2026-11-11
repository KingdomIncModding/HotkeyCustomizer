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
        internal void BuildContent(IModConfigContext context)
        {
            Begin();
            Dropdowns.Clear();
            KeyButtons.Clear();
            BuildHeader(context);
            BuildChoices();
            for (int slot = 0; slot < Pending.Length; slot++) BuildRow(context, slot);
            var row = context.Templates.CreateButtonRow(context.Container, "Defaults", "Reset All Shortcuts",
                ResetChoices, out _, out _);
            row.name = "ResetRow";
        }
    }
}
