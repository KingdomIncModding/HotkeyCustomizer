using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Flow;
using Assets.Source.Player;
using Assets.Source.Util;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace KingdomHotkeyCustomizer
{
    public sealed class BuildingSettings : MonoBehaviour
    {
        internal readonly string[] Pending = new string[10];
        internal readonly string[] PendingKeys = new string[10];
        internal readonly List<TMP_Dropdown> Dropdowns = new List<TMP_Dropdown>();
        internal readonly List<Button> KeyButtons = new List<Button>();
        internal readonly Dictionary<string, string> PendingVanilla = new Dictionary<string, string>();
        internal bool PendingShowHotbar;
        internal Toggle HotbarToggle;
        internal TMP_Text SectionLabel;
        internal InputAction ListeningAction;
        TMP_Text nativeLabel;
        string nativeText;
        Color nativeColor;
        internal int Listening = -1;
        int listenFrame;
        internal List<string> Choices;

        internal void Begin()
        {
            CancelListening();
            PendingShowHotbar = HotkeyCustomizerPlugin.Instance.ShowHotbar.Value;
            PendingVanilla.Clear();
            foreach (var action in GameInput.RebindableKeys) PendingVanilla[action.name] = VanillaBindings.Entry(action).Value;
            for (int digit = 0; digit < 10; digit++) {
                Pending[digit] = HotkeyCustomizerPlugin.Instance.Buildings[digit].Value;
                PendingKeys[digit] = HotkeyCustomizerPlugin.Instance.Bindings[digit].Value;
            }
        }

        internal void Commit()
        {
            var plugin = HotkeyCustomizerPlugin.Instance;
            bool saveOnSet = plugin.Config.SaveOnConfigSet;
            plugin.Config.SaveOnConfigSet = false;
            CancelListening();
            try { for (int digit = 0; digit < 10; digit++) {
                plugin.Buildings[digit].Value = Pending[digit] ?? "";
                plugin.Bindings[digit].Value = PendingKeys[digit] ?? "";
            }
                foreach (var action in GameInput.RebindableKeys) VanillaBindings.Entry(action).Value = PendingVanilla[action.name];
                plugin.ShowHotbar.Value = PendingShowHotbar;
            }
            finally { plugin.Config.SaveOnConfigSet = saveOnSet; }
            plugin.Config.Save();
        }

        internal void ResetChoices()
        {
            CancelListening();
            PendingShowHotbar = true;
            for (int digit = 0; digit < 10; digit++) { Pending[digit] = ""; PendingKeys[digit] = ""; }
            foreach (var name in PendingVanilla.Keys.ToArray()) PendingVanilla[name] = "";
        }

        void OnDisable() => CancelListening();
        internal void Listen(int slot)
        {
            CancelListening();
            GetComponent<KeyBindingsUI>().SetListening(null);
            Listening = slot; listenFrame = Time.frameCount;
            KeyButtons[slot].GetComponentInChildren<TMP_Text>().text = "Press a key...";
            KeyButtons[slot].GetComponentInChildren<TMP_Text>().color = Color.green;
        }
        internal void ListenNative(InputAction action, TMP_Text label)
        {
            CancelListening();
            GetComponent<KeyBindingsUI>().SetListening(null);
            ListeningAction = action; nativeLabel = label; nativeText = label.text; nativeColor = label.color;
            listenFrame = Time.frameCount;
            label.text = "Press a key..."; label.color = Color.green;
        }
        internal void CancelListening()
        {
            if (Listening >= 0 && Listening < KeyButtons.Count && KeyButtons[Listening])
            {
                var label = KeyButtons[Listening].GetComponentInChildren<TMP_Text>();
                label.text = BuildingShortcut.Parse(PendingKeys[Listening]).Display;
                label.color = keyColor;
            }
            if (nativeLabel) { nativeLabel.text = nativeText; nativeLabel.color = nativeColor; }
            ListeningAction = null; nativeLabel = null;
            Listening = -1;
        }
        Color keyColor;
        void Update()
        {
            if ((Listening < 0 && ListeningAction == null) || Time.frameCount == listenFrame || Keyboard.current == null) return;
            var keyboard = Keyboard.current;
            foreach (var key in keyboard.allKeys)
            {
                if (!key.wasPressedThisFrame || BuildingShortcut.Modifier(key.keyCode)) continue;
                if (key.keyCode == Key.Escape) { CancelListening(); return; }
                string value = key.keyCode == Key.Backspace || key.keyCode == Key.Delete
                    ? "" : BuildingShortcut.Capture(keyboard, key.keyCode).ToString();
                if (ListeningAction != null)
                {
                    var action = ListeningAction; var label = nativeLabel;
                    PendingVanilla[action.name] = value == "" ? VanillaBindings.Unassigned : value;
                    VanillaBindings.Apply(action, PendingVanilla[action.name]);
                    CancelListening(); label.text = BuildingShortcut.Parse(value).Display; return;
                }
                PendingKeys[Listening] = value;
                CancelListening(); return;
            }
        }

        internal void Build(RectTransform parent, KeyBindingRow prefab)
        {
            CancelListening(); Dropdowns.Clear(); KeyButtons.Clear();
            // The resolution dropdown is available in both menu scenes. Clone its
            // complete native template, fonts, scrolling and pointer behavior.
            var template = Resources.FindObjectsOfTypeAll<OptionsUI>()
                .Where(o => o.gameObject.scene == gameObject.scene)
                .Select(o => (TMP_Dropdown)AccessTools.Field(typeof(OptionsUI), "_resolutionsDropdown").GetValue(o))
                .FirstOrDefault(d => d);
            if (!template) throw new InvalidOperationException("Native options dropdown was not available.");
            var options = Resources.FindObjectsOfTypeAll<OptionsUI>().First(o => o.gameObject.scene == gameObject.scene
                && (TMP_Dropdown)AccessTools.Field(typeof(OptionsUI), "_resolutionsDropdown").GetValue(o) == template);
            BuildSection(parent, prefab, options);
            var elements = BuildingCatalog.All()
                .OrderBy(e => Translation.Translate(e.Label), StringComparer.CurrentCultureIgnoreCase).ToList();
            Choices = new List<string> { "" };
            Choices.AddRange(elements.Select(e => e.ElementType));
            // Keep assignments from an absent companion mod instead of silently erasing them.
            Choices.AddRange(Pending.Where(id => !string.IsNullOrEmpty(id) && !Choices.Contains(id)).Distinct());
            var labels = new List<string> { "Unassigned" };
            labels.AddRange(elements.Select(e => Translation.Translate(e.Label)
                + (GamePlayer.Current != null && !e.IsPurchaseable(GamePlayer.Current) ? " (locked)" : "")));
            labels.AddRange(Choices.Skip(elements.Count + 1).Select(id => id + " (unavailable)"));

            for (int digit = 0; digit < 10; digit++)
            {
                var clone = Instantiate(prefab, parent);
                clone.name = "Building hotkey " + digit;
                var label = (TMP_Text)AccessTools.Field(typeof(KeyBindingRow), "_label").GetValue(clone);
                var binding = (TMP_Text)AccessTools.Field(typeof(KeyBindingRow), "_binding").GetValue(clone);
                label.text = "Building " + digit;
                label.rectTransform.anchorMin = new Vector2(0, 0); label.rectTransform.anchorMax = new Vector2(.20f, 1);
                label.rectTransform.offsetMin = new Vector2(6, 0); label.rectTransform.offsetMax = Vector2.zero;
                keyColor = binding.color;
                var nativeButton = (Image)AccessTools.Field(typeof(KeyBindingRow), "_button").GetValue(clone);
                var row = (RectTransform)clone.transform;
                var buttonRect = (RectTransform)new GameObject("Assign key", typeof(RectTransform), typeof(Image), typeof(Button)).transform;
                buttonRect.SetParent(row, false);
                buttonRect.anchorMin = new Vector2(.68f, 0); buttonRect.anchorMax = new Vector2(1, 1);
                buttonRect.offsetMin = Vector2.zero; buttonRect.offsetMax = Vector2.zero;
                var buttonImage = buttonRect.GetComponent<Image>();
                buttonImage.sprite = nativeButton.sprite; buttonImage.type = nativeButton.type;
                buttonImage.color = new Color(0, 0, 0, .5f);
                var keyButton = buttonRect.GetComponent<Button>(); keyButton.targetGraphic = buttonImage;
                keyButton.transition = Selectable.Transition.None;
                buttonRect.gameObject.AddComponent<NativeKeyHover>().Target = buttonImage;
                var keyLabel = binding; keyLabel.transform.SetParent(buttonRect, false);
                keyLabel.rectTransform.anchorMin = Vector2.zero; keyLabel.rectTransform.anchorMax = Vector2.one;
                keyLabel.rectTransform.offsetMin = new Vector2(6, 0); keyLabel.rectTransform.offsetMax = new Vector2(-6, 0);
                keyLabel.enableAutoSizing = true; keyLabel.fontSizeMin = 12; keyLabel.fontSizeMax = keyLabel.fontSize;
                keyLabel.text = BuildingShortcut.Parse(PendingKeys[digit]).Display;
                keyLabel.raycastTarget = false;
                int slot = digit; keyButton.onClick.AddListener(() => Listen(slot)); KeyButtons.Add(keyButton);
                var dropdown = Instantiate(template, row);
                dropdown.name = "Building " + digit;
                dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
                dropdown.navigation = new Navigation { mode = Navigation.Mode.None };
                dropdown.ClearOptions(); dropdown.AddOptions(labels);
                var rect = (RectTransform)dropdown.transform;
                rect.anchorMin = new Vector2(.20f, 0); rect.anchorMax = new Vector2(.68f, 1);
                rect.offsetMin = new Vector2(4, 3); rect.offsetMax = new Vector2(-4, -3);
                // The game's row layout must ignore the nested dropdown's old preferred size.
                var layout = dropdown.GetComponent<LayoutElement>(); if (layout) layout.ignoreLayout = true;
                dropdown.captionText.font = label.font; dropdown.captionText.color = label.color;
                dropdown.captionText.fontSize = label.fontSize;
                dropdown.captionText.enableAutoSizing = true;
                dropdown.captionText.fontSizeMin = 12; dropdown.captionText.fontSizeMax = label.fontSize;
                dropdown.itemText.fontSize = 18;
                dropdown.itemText.enableAutoSizing = true;
                dropdown.itemText.fontSizeMin = 12; dropdown.itemText.fontSizeMax = 18;
                dropdown.template.sizeDelta = new Vector2(dropdown.template.sizeDelta.x, 260);
                dropdown.SetValueWithoutNotify(Math.Max(0, Choices.IndexOf(Pending[digit] ?? "")));
                dropdown.RefreshShownValue();
                dropdown.onValueChanged.AddListener(index => { CancelListening(); Pending[slot] = Choices[index]; });
                dropdown.gameObject.SetActive(true);
                // A building selector is a dropdown, so it must not run the cloned row's rebind listener.
                DestroyImmediate(clone);
                Dropdowns.Add(dropdown);
            }
            // Preserve the game's scrolling layout and make every appended row reachable.
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        }

        void BuildSection(RectTransform parent, KeyBindingRow prefab, OptionsUI options)
        {
            var heading = Instantiate(prefab, parent);
            heading.name = "Custom Hotkeys section";
            SectionLabel = (TMP_Text)AccessTools.Field(typeof(KeyBindingRow), "_label").GetValue(heading);
            SectionLabel.text = "Custom Hotkeys";
            SectionLabel.fontStyle = FontStyles.Bold;
            SectionLabel.rectTransform.anchorMax = Vector2.one;
            SectionLabel.rectTransform.offsetMin = new Vector2(6, 0);
            SectionLabel.rectTransform.offsetMax = Vector2.zero;
            ((TMP_Text)AccessTools.Field(typeof(KeyBindingRow), "_binding").GetValue(heading)).gameObject.SetActive(false);
            var headingImage = (Image)AccessTools.Field(typeof(KeyBindingRow), "_button").GetValue(heading);
            headingImage.enabled = false;
            var headingLayout = heading.GetComponent<LayoutElement>() ?? heading.gameObject.AddComponent<LayoutElement>();
            headingLayout.preferredHeight = 34; headingLayout.minHeight = 34;
            DestroyImmediate(heading);

            var row = Instantiate(prefab, parent);
            row.name = "Show Hotbar";
            var label = (TMP_Text)AccessTools.Field(typeof(KeyBindingRow), "_label").GetValue(row);
            label.text = "Show Hotbar"; label.raycastTarget = false;
            ((Image)AccessTools.Field(typeof(KeyBindingRow), "_button").GetValue(row)).enabled = false;
            ((TMP_Text)AccessTools.Field(typeof(KeyBindingRow), "_binding").GetValue(row)).gameObject.SetActive(false);
            var source = (Toggle)AccessTools.Field(typeof(OptionsUI), "_windowModeToggle").GetValue(options);
            HotbarToggle = Instantiate(source, row.transform);
            HotbarToggle.name = "Show Hotbar checkbox";
            HotbarToggle.onValueChanged = new Toggle.ToggleEvent();
            HotbarToggle.group = null;
            HotbarToggle.navigation = new Navigation { mode = Navigation.Mode.None };
            foreach (var text in HotbarToggle.GetComponentsInChildren<TMP_Text>(true)) text.gameObject.SetActive(false);
            var rect = (RectTransform)HotbarToggle.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.84f, .5f);
            rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one; rect.sizeDelta = new Vector2(24, 24);
            var background = HotbarToggle.targetGraphic.rectTransform;
            background.anchorMin = Vector2.zero; background.anchorMax = Vector2.one;
            background.offsetMin = background.offsetMax = Vector2.zero;
            var checkmark = HotbarToggle.graphic.rectTransform;
            checkmark.anchorMin = Vector2.zero; checkmark.anchorMax = Vector2.one;
            checkmark.offsetMin = checkmark.offsetMax = Vector2.zero;
            HotbarToggle.SetIsOnWithoutNotify(PendingShowHotbar);
            HotbarToggle.onValueChanged.AddListener(value => { CancelListening(); PendingShowHotbar = value; });
            HotbarToggle.gameObject.SetActive(true);
            DestroyImmediate(row);
        }
    }
}
