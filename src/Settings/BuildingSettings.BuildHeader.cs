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
        private void BuildHeader(IModConfigContext context)
        {
            if (FoundationApi.World.Player != null && !HotkeyCustomizerPlugin.Unlocked)
            {
                context.Templates.CreateTextRow(context.Container, "Research Hotkey Customizer for free in Tech Tree > Mods.", false, out ResearchNotice);
                if (ResearchNotice)
                {
                    ResearchNotice.color = new Color(1f, .85f, .4f);
                    ResearchNotice.fontSize = 15;
                }
            }
            context.Templates.CreateToggleRow(context.Container, "Show Hotbar", PendingShowHotbar,
                value => { CancelListening(); PendingShowHotbar = value; }, out HotbarToggle);
            context.Templates.CreateTextRow(context.Container, "Custom Hotkeys", true, out _);
        }
    }
}
