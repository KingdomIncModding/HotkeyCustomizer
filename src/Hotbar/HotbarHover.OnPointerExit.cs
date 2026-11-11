using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class HotbarHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerExit(PointerEventData data) => Owner.Hovered = false;
    }
}
