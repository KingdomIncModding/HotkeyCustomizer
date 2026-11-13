using System.Linq;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed class BuildingHotbar : MonoBehaviour
    {
        internal void Build(UnityEngine.Component ui)
        {
            var flow = FoundationApi.UiAccess.WorkspaceSwitcher;
            var prefab = FoundationApi.UiAccess.WorkspaceItemTemplate;
            canvas = (RectTransform)flow.GetComponentInParent<Canvas>().rootCanvas.transform;
            Strip = (RectTransform)new GameObject("Custom Hotkeys hotbar", typeof(RectTransform), typeof(Image), typeof(HotbarHover)).transform;
            Strip.SetParent(canvas, false);
            Strip.anchorMin = Strip.anchorMax = new Vector2(1, .5f);
            Strip.pivot = new Vector2(1, .5f);
            Strip.GetComponent<Image>().color = Color.black;
            Strip.GetComponent<HotbarHover>().Owner = this;
            for (int slot = 0; slot < Items.Length; slot++)
                BuildItem(prefab, slot);
            Refresh();
        }

        private void BuildItem(UnityEngine.Component prefab, int slot)
        {
            var tile = Instantiate(prefab, Strip);
            tile.name = "Custom Hotkey " + slot;
            var icon = FoundationApi.UiAccess.WorkspaceItemIcon(tile);
            var key = FoundationApi.UiAccess.WorkspaceItemIndex(tile);
            var highlight = FoundationApi.UiAccess.WorkspaceItemHighlight(tile);
            highlight.SetActive(false);
            DestroyImmediate(tile); // Remove the native workspace handler; clicks select only a building.
            var root = (RectTransform)Strip.GetChild(slot);
            var background = root.GetComponent<Image>();
            background.sprite = null;
            background.type = Image.Type.Simple;
            background.color = Color.black;
            FoundationApi.UiAccess.RemoveNativeTooltips(root.gameObject);
            root.gameObject.AddComponent<FoundationTooltipSource>().SetSource(root.gameObject);
            var item = root.gameObject.AddComponent<BuildingHotbarItem>();
            item.Slot = slot;
            item.Icon = icon;
            item.KeyLabel = key;
            Items[slot] = item;
            var rect = icon.rectTransform;
            rect.anchorMin = new Vector2(.14f, .27f);
            rect.anchorMax = new Vector2(.86f, .94f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            key.rectTransform.anchorMin = new Vector2(0, .02f);
            key.rectTransform.anchorMax = new Vector2(1, .28f);
            key.rectTransform.offsetMin = new Vector2(2, 0);
            key.rectTransform.offsetMax = new Vector2(-2, 0);
            key.enableAutoSizing = true;
            key.fontSizeMin = 8;
            key.fontSizeMax = 14;
            key.alignment = TextAlignmentOptions.Center;
            key.raycastTarget = false;
            root.gameObject.SetActive(true);
        }

        internal RectTransform Strip;
        internal readonly BuildingHotbarItem[] Items = new BuildingHotbarItem[10];
        internal bool Hovered;
        internal const float Peek = 14;
        private float revealed;
        private RectTransform canvas;
        private void LateUpdate() => Refresh();
        private void OnDestroy()
        {
            if (Strip)
                Destroy(Strip.gameObject);
        }

        internal void Refresh()
        {
            if (!Strip || !HotkeyCustomizerPlugin.Instance)
                return;
            var plugin = HotkeyCustomizerPlugin.Instance;
            bool visible = plugin.ShowHotbar.Value && HotkeyCustomizerPlugin.Unlocked && FoundationApi.UiAccess.Root && FoundationApi.UiAccess.IsWorkspaceVisible && Time.timeScale > 0 && !FoundationApi.UiAccess.IsMenuVisible && !FoundationApi.UiAccess.IsAlertVisible;
            Strip.gameObject.SetActive(visible);
            if (!visible)
            {
                Hovered = false;
                revealed = 0;
                return;
            }

            float size = Mathf.Clamp((canvas.rect.height - 180) / 10, 24, 54);
            Strip.sizeDelta = new Vector2(size + 8, size * 10 + 8);
            revealed = Mathf.MoveTowards(revealed, Hovered ? 1 : 0, Time.unscaledDeltaTime * (Hovered ? 10 : 4));
            Strip.anchoredPosition = new Vector2((Strip.rect.width - Peek) * (1 - revealed), 0);
            for (int slot = 0; slot < Items.Length; slot++)
            {
                var rect = (RectTransform)Items[slot].transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(4, -4 - slot * size);
                rect.sizeDelta = new Vector2(size, size);
                Items[slot].Refresh();
            }
        }
    }
}
