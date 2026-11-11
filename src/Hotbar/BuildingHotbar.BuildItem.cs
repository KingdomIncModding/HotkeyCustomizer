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
    public sealed partial class BuildingHotbar : MonoBehaviour
    {
        private void BuildItem(SwapFlowItem prefab, int slot)
        {
            var tile = Instantiate(prefab, Strip);
            tile.name = "Custom Hotkey " + slot;
            var icon = (Image)AccessTools.Field(typeof(SwapFlowItem), "_icon").GetValue(tile);
            var key = (TMP_Text)AccessTools.Field(typeof(SwapFlowItem), "_index").GetValue(tile);
            var highlight = (GameObject)AccessTools.Field(typeof(SwapFlowItem), "_highlight").GetValue(tile);
            highlight.SetActive(false);
            DestroyImmediate(tile); // Remove the native workspace handler; clicks select only a building.
            var root = (RectTransform)Strip.GetChild(slot);
            var background = root.GetComponent<Image>();
            background.sprite = null;
            background.type = Image.Type.Simple;
            background.color = Color.black;
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
    }
}
