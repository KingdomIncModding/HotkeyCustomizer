using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomHotkeyCustomizer
{
    internal sealed class ModHotkeyView : MonoBehaviour
    {
        private void OnDisable() => ModHotkeyCapture.Instance?.CancelIn(transform);
    }
}
