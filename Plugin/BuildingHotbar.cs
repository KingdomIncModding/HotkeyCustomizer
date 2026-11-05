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
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    // Own the strip from GameUI so it can reappear after its setting is toggled.
    public sealed class BuildingHotbar : MonoBehaviour
    {
        internal RectTransform Strip;
        internal readonly BuildingHotbarItem[] Items = new BuildingHotbarItem[10];
        internal bool Hovered;
        internal const float Peek = 14;
        float revealed;
        RectTransform canvas;

        internal void Build(GameUI ui)
        {
            var flow = (SwapFlowMenu)AccessTools.Field(typeof(GameUI), "_flowMenu").GetValue(ui);
            var prefab = (SwapFlowItem)AccessTools.Field(typeof(SwapFlowMenu), "_itemPrefab").GetValue(flow);
            canvas = (RectTransform)flow.GetComponentInParent<Canvas>().rootCanvas.transform;
            Strip = (RectTransform)new GameObject("Custom Hotkeys hotbar", typeof(RectTransform), typeof(Image), typeof(HotbarHover)).transform;
            Strip.SetParent(canvas, false);
            Strip.anchorMin = Strip.anchorMax = new Vector2(1, .5f);
            Strip.pivot = new Vector2(1, .5f);
            Strip.GetComponent<Image>().color = Color.black;
            Strip.GetComponent<HotbarHover>().Owner = this;
            for (int slot = 0; slot < Items.Length; slot++)
            {
                var tile = Instantiate(prefab, Strip);
                tile.name = "Custom Hotkey " + slot;
                var icon = (Image)AccessTools.Field(typeof(SwapFlowItem), "_icon").GetValue(tile);
                var key = (TMP_Text)AccessTools.Field(typeof(SwapFlowItem), "_index").GetValue(tile);
                var highlight = (GameObject)AccessTools.Field(typeof(SwapFlowItem), "_highlight").GetValue(tile);
                highlight.SetActive(false);
                DestroyImmediate(tile);
                // Replace the native workspace handler completely: a tile only selects its building.
                var root = (RectTransform)Strip.GetChild(slot);
                var background = root.GetComponent<Image>();
                background.sprite = null;
                background.type = Image.Type.Simple;
                background.color = Color.black;
                var item = root.gameObject.AddComponent<BuildingHotbarItem>();
                item.Slot = slot; item.Icon = icon; item.KeyLabel = key;
                Items[slot] = item;
                var rect = icon.rectTransform;
                rect.anchorMin = new Vector2(.14f, .27f); rect.anchorMax = new Vector2(.86f, .94f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                icon.preserveAspect = true; icon.raycastTarget = false;
                key.rectTransform.anchorMin = new Vector2(0, .02f); key.rectTransform.anchorMax = new Vector2(1, .28f);
                key.rectTransform.offsetMin = new Vector2(2, 0); key.rectTransform.offsetMax = new Vector2(-2, 0);
                key.enableAutoSizing = true; key.fontSizeMin = 8; key.fontSizeMax = 14;
                key.alignment = TextAlignmentOptions.Center; key.raycastTarget = false;
                root.gameObject.SetActive(true);
            }
            Refresh();
        }

        void LateUpdate() => Refresh();

        internal void Refresh()
        {
            if (!Strip || !HotkeyCustomizerPlugin.Instance) return;
            var plugin = HotkeyCustomizerPlugin.Instance;
            bool visible = plugin.ShowHotbar.Value && HotkeyCustomizerPlugin.Unlocked && GameUI.Instance
                && WorldFlowManager.Visible && Time.timeScale > 0 && !GameUI.MenuVisible && !UIAlertWindow.Active;
            Strip.gameObject.SetActive(visible);
            if (!visible) { Hovered = false; revealed = 0; return; }
            float size = Mathf.Clamp((canvas.rect.height - 180) / 10, 24, 54);
            Strip.sizeDelta = new Vector2(size + 8, size * 10 + 8);
            // Like the workspace toolbar: quick reveal, gentler retreat, no jump on reversal.
            revealed = Mathf.MoveTowards(revealed, Hovered ? 1 : 0, Time.unscaledDeltaTime * (Hovered ? 10 : 4));
            Strip.anchoredPosition = new Vector2((Strip.rect.width - Peek) * (1 - revealed), 0);
            for (int slot = 0; slot < Items.Length; slot++)
            {
                var rect = (RectTransform)Items[slot].transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(4, -4 - slot * size); rect.sizeDelta = new Vector2(size, size);
                Items[slot].Refresh();
            }
        }

        void OnDestroy() { if (Strip) Destroy(Strip.gameObject); }
    }

    public sealed class HotbarHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        internal BuildingHotbar Owner;
        public void OnPointerEnter(PointerEventData data) => Owner.Hovered = true;
        public void OnPointerExit(PointerEventData data) => Owner.Hovered = false;
    }

    public sealed class BuildingHotbarItem : MonoBehaviour, IPointerClickHandler, ITooltipTitleSource, ITooltipCustomSource
    {
        internal int Slot;
        internal Image Icon;
        internal TMP_Text KeyLabel;
        FlowElement element;
        string buildingId;

        internal void Refresh()
        {
            var plugin = HotkeyCustomizerPlugin.Instance;
            string id = plugin.Buildings[Slot].Value;
            if (id != buildingId)
            {
                buildingId = id;
                element = FlowElement.GetAllTypes().FirstOrDefault(e => e.ElementType == id && e.RequiredTech != null);
                Icon.sprite = element?.Icon;
            }
            Icon.gameObject.SetActive(Icon.sprite);
            bool unlocked = element != null && element.IsPurchaseable(GamePlayer.Current);
            Icon.color = unlocked ? Color.white : new Color(1, 1, 1, .35f);
            string key = plugin.Shortcuts[Slot].Display;
            KeyLabel.text = key == "Unassigned" ? "—" : key;
        }

        public void OnPointerClick(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left) HotkeyCustomizerPlugin.Instance.SelectBuilding(Slot);
        }

        public string GetTooltipTitle() => element != null ? Translation.Translate(element.Label)
            : string.IsNullOrEmpty(buildingId) ? "Unassigned" : buildingId + " (unavailable)";

        public void AddTooltipCustomContent(UITooltip tooltip)
        {
            tooltip.AddTextLine("Building " + Slot + " · " + HotkeyCustomizerPlugin.Instance.Shortcuts[Slot].Display);
            if (element != null && !element.IsPurchaseable(GamePlayer.Current)) tooltip.AddTextLine("Research required");
            else if (element != null) tooltip.AddTextLine("Click to select building");
            else tooltip.AddTextLine("Assign a building in Options > Key Bindings > Custom Hotkeys");
        }
    }
}
