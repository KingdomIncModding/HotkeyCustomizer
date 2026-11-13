using KingdomFoundation.Api;
using KingdomHotkeyCustomizer.Api;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal sealed class ModHotkeyCapture : MonoBehaviour
    {
        internal void Cancel()
        {
            if (activeLabel)
            {
                activeLabel.color = labelColor;
                activeLabel.text = BuildingShortcut.Parse(Current?.PendingValue).Display;
            }

            Current = null;
            activeLabel = null;
        }

        internal void CancelFor(ModHotkeySection owner)
        {
            if (Current?.Owner == owner)
                Cancel();
        }

        internal void CancelIn(Transform root)
        {
            if (activeLabel && activeLabel.transform.IsChildOf(root))
                Cancel();
        }

        internal static ModHotkeyCapture Instance { get; private set; }
        internal ModHotkey Current { get; private set; }
        internal bool IsListening => Current != null;

        private TMP_Text activeLabel;
        private Color labelColor;
        private int listenFrame;
        internal int EscapeFrame { get; set; } = -1;
        internal static bool BlocksEscape => Instance?.IsListening == true || Instance?.EscapeFrame == Time.frameCount || BuildingSettings.Instance?.Listening >= 0 || BuildingSettings.Instance?.ListeningAction != null;

        private void Awake() => Instance = this;
        private void OnDestroy()
        {
            Cancel();
            if (Instance == this)
                Instance = null;
        }

        internal void Listen(ModHotkey key, TMP_Text label)
        {
            Cancel();
            BuildingSettings.Instance?.CancelListening();
            if (!key.IsRegistered || !label || !label.gameObject.activeInHierarchy)
                return;
            Current = key;
            activeLabel = label;
            labelColor = label.color;
            listenFrame = Time.frameCount;
            label.text = "Press a key...";
            label.color = Color.green;
        }

        private void Update()
        {
            if (Current == null)
                return;
            if (!Current.IsRegistered || !activeLabel || !activeLabel.gameObject.activeInHierarchy)
            {
                Cancel();
                return;
            }

            if (Time.frameCount == listenFrame || Keyboard.current == null)
                return;
            foreach (var key in Keyboard.current.allKeys)
            {
                if (!key.wasPressedThisFrame || BuildingShortcut.Modifier(key.keyCode))
                    continue;
                if (key.keyCode == Key.Escape)
                    EscapeFrame = Time.frameCount;
                else
                    Current.PendingValue = key.keyCode == Key.Backspace || key.keyCode == Key.Delete ? "" : BuildingShortcut.Capture(Keyboard.current, key.keyCode).ToString();
                Cancel();
                return;
            }
        }
    }
}
