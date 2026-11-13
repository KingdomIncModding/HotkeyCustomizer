using System.Linq;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed class BuildingHotbarItem : MonoBehaviour, IPointerClickHandler, IFoundationTooltipTitleSource, IFoundationTooltipCustomSource
    {
        public void AddTooltipCustomContent(IFoundationTooltipContent tooltip)
        {
            tooltip.AddTextLine("Building " + Slot + " · " + HotkeyCustomizerPlugin.Instance.Shortcuts[Slot].Display);
            if (element != null && !FoundationApi.WorkspaceAccess.CanPurchase(element))
                tooltip.AddTextLine("Research required");
            else if (element != null)
                tooltip.AddTextLine("Click to select building");
            else
                tooltip.AddTextLine("Assign a building in Options > Mods > Hotkey Customizer");
        }

        internal int Slot;
        internal Image Icon;
        internal TMP_Text KeyLabel;
        private WorkspaceObject element;
        private string buildingId;
        public string GetTooltipTitle() => element != null ? FoundationApi.Presentation.Translate(element.Label) : string.IsNullOrEmpty(buildingId) ? "Unassigned" : buildingId + " (unavailable)";
        public void OnPointerClick(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left)
                HotkeyCustomizerPlugin.Instance.SelectBuilding(Slot);
        }

        internal void Refresh()
        {
            var plugin = HotkeyCustomizerPlugin.Instance;
            string id = plugin.Buildings[Slot].Value;
            if (id != buildingId)
            {
                buildingId = id;
                element = FoundationApi.WorkspaceAccess.Catalog.FirstOrDefault(e => e.TypeId == id && e.RequiredResearch != null);
                Icon.sprite = element?.Icon;
            }

            Icon.gameObject.SetActive(Icon.sprite);
            bool unlocked = element != null && FoundationApi.WorkspaceAccess.CanPurchase(element);
            Icon.color = unlocked ? Color.white : new Color(1, 1, 1, .35f);
            string key = plugin.Shortcuts[Slot].Display;
            KeyLabel.text = key == "Unassigned" ? "—" : key;
        }
    }
}
