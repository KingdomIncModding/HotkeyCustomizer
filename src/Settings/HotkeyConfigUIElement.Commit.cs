using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    internal sealed partial class HotkeyConfigUIElement : IModConfigElement
    {
        public void Commit() => settings.Commit();
    }
}
