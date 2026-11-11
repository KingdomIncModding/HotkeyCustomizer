using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class NativeKeyHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        internal Image Target;
    }
}
