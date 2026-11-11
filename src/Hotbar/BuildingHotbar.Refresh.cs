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
