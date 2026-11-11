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
            for (int slot = 0; slot < Items.Length; slot++) BuildItem(prefab, slot);
            Refresh();
        }
    }
}
