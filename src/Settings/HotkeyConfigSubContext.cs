using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomHotkeyCustomizer
{
    internal sealed class HotkeyConfigSubContext : IModConfigContext
    {
        public HotkeyConfigSubContext(RectTransform container, IModConfigTemplates templates)
        {
            Container = container;
            Templates = templates;
        }

        public RectTransform Container { get; }
        public IModConfigTemplates Templates { get; }
    }
}
