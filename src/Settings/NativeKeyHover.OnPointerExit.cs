using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class NativeKeyHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerExit(PointerEventData data) => Target.color = new Color(0, 0, 0, .5f);
    }
}
