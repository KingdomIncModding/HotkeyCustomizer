using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed class HotbarHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        internal BuildingHotbar Owner;
        public void OnPointerEnter(PointerEventData data) => Owner.Hovered = true;
        public void OnPointerExit(PointerEventData data) => Owner.Hovered = false;
    }
}
