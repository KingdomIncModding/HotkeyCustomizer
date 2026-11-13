using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed class NativeKeyHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        internal Image Target;
        public void OnPointerEnter(PointerEventData data) => Target.color = new Color(.5f, .5f, .5f, .5f);
        public void OnPointerExit(PointerEventData data) => Target.color = new Color(0, 0, 0, .5f);
    }
}
