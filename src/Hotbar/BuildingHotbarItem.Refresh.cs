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
        internal void Refresh()
        {
            var plugin = HotkeyCustomizerPlugin.Instance;
            string id = plugin.Buildings[Slot].Value;
            if (id != buildingId)
            {
                buildingId = id;
                element = FoundationApi.Machines.Catalog.FirstOrDefault(e => e.ElementType == id && e.RequiredTech != null);
                Icon.sprite = element?.Icon;
            }
            Icon.gameObject.SetActive(Icon.sprite);
            bool unlocked = element != null && element.IsPurchaseable(FoundationApi.World.Player);
            Icon.color = unlocked ? Color.white : new Color(1, 1, 1, .35f);
            string key = plugin.Shortcuts[Slot].Display;
            KeyLabel.text = key == "Unassigned" ? "—" : key;
        }
    }
}
