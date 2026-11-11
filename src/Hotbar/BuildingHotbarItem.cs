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
        internal int Slot;
        internal Image Icon;
        internal TMP_Text KeyLabel;
        private FlowElement element;
        private string buildingId;
    }
}
