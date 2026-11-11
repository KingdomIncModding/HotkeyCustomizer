using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    internal sealed partial class HotkeyConfigUIElement : IModConfigElement
    {
        public string Id => "urakka.hotkeys.settings";
        public bool IsVisible { get; set; } = true;
        private readonly BuildingSettings settings;
    }
}
