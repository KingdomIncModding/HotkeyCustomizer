using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    internal sealed partial class HotkeyConfigUIElement : IModConfigElement
    {
        public GameObject CreateView(IModConfigContext context)
        {
            var container = new GameObject("HotkeyCustomizerControls", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            container.transform.SetParent(context.Container, false);
            var layout = container.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            container.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            settings.BuildContent(new HotkeyConfigSubContext((RectTransform)container.transform, context.Templates));
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)container.transform);
            return container;
        }
    }
}
