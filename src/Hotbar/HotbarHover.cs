using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class HotbarHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        internal BuildingHotbar Owner;
    }
}
