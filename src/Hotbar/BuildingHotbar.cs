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
        internal RectTransform Strip;
        internal readonly BuildingHotbarItem[] Items = new BuildingHotbarItem[10];
        internal bool Hovered;
        internal const float Peek = 14;
        private float revealed;
        private RectTransform canvas;
    }
}
