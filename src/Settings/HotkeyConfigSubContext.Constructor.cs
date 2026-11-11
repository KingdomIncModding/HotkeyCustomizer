using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomHotkeyCustomizer
{
    internal sealed partial class HotkeyConfigSubContext : IModConfigContext
    {
        public HotkeyConfigSubContext(RectTransform container, IModConfigTemplates templates)
        {
            Container = container;
            Templates = templates;
        }
    }
}
