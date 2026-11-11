using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomHotkeyCustomizer
{
    internal sealed partial class HotkeyConfigSubContext : IModConfigContext
    {
        public RectTransform Container { get; }
        public IModConfigTemplates Templates { get; }
    }
}
