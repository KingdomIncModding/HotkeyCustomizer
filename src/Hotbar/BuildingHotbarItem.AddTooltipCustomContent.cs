using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.SwapFlow;
using Assets.Source.Flow;
using Assets.Source.Player;
using Assets.Source.Util;
using Behaviour.UI;
using Behaviour.UI.Tooltip;
using HarmonyLib;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class BuildingHotbarItem : MonoBehaviour, IPointerClickHandler, ITooltipTitleSource, ITooltipCustomSource
    {
        public void AddTooltipCustomContent(UITooltip tooltip)
        {
            tooltip.AddTextLine("Building " + Slot + " · " + HotkeyCustomizerPlugin.Instance.Shortcuts[Slot].Display);
            if (element != null && !element.IsPurchaseable(FoundationApi.World.Player)) tooltip.AddTextLine("Research required");
            else if (element != null) tooltip.AddTextLine("Click to select building");
            else tooltip.AddTextLine("Assign a building in Options > Mods > Hotkey Customizer");
        }
    }
}
