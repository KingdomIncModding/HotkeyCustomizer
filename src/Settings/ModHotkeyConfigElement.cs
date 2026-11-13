using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    internal sealed class ModHotkeyConfigElement : IModConfigElement
    {
        public GameObject CreateView(IModConfigContext context)
        {
            var root = new GameObject(owner.ModId + ".hotkeys", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(ModHotkeyView));
            root.transform.SetParent(context.Container, false);
            var layout = root.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            root.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            owner.BuildView(new HotkeyConfigSubContext((RectTransform)root.transform, context.Templates));
            root.SetActive(IsVisible);
            return root;
        }

        private readonly Api.ModHotkeySection owner;
        public string Id => owner.ModId + ".hotkeys";
        public bool IsVisible { get; set; } = true;

        internal ModHotkeyConfigElement(Api.ModHotkeySection owner)
        {
            this.owner = owner;
        }

        public void Commit() => owner.Commit();
        public void Cancel() => owner.Cancel();
    }
}
