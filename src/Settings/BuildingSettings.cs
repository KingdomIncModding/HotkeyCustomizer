using System;
using System.Collections.Generic;
using System.Linq;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomHotkeyCustomizer
{
    public sealed class BuildingSettings : MonoBehaviour
    {
        private void Awake()
        {
            Instance = this;
            Begin();
            RegisterModConfig();
        }

        internal void Begin()
        {
            CancelListening();
            var plugin = HotkeyCustomizerPlugin.Instance;
            PendingShowHotbar = plugin != null && plugin.ShowHotbar.Value;
            PendingVanilla.Clear();
            if (FoundationApi.Bindings.Rebindable != null)
                foreach (var action in FoundationApi.Bindings.Rebindable)
                    PendingVanilla[action.name] = VanillaBindings.Entry(action).Value;
            for (int slot = 0; slot < Pending.Length; slot++)
            {
                Pending[slot] = plugin != null ? plugin.Buildings[slot].Value : "";
                PendingKeys[slot] = plugin != null ? plugin.Bindings[slot].Value : "";
            }
        }

        private void BuildChoices()
        {
            var elements = BuildingCatalog.All().OrderBy(e => FoundationApi.Presentation.Translate(e.Label), StringComparer.CurrentCultureIgnoreCase).ToList();
            Choices = new List<string>
            {
                ""
            };
            Choices.AddRange(elements.Select(e => e.TypeId));
            Choices.AddRange(Pending.Where(id => !string.IsNullOrEmpty(id) && !Choices.Contains(id)).Distinct());
            // Native IDs and the related display labels must have matching indexes.
            choiceLabels = new List<string>
            {
                "Unassigned"
            };
            choiceLabels.AddRange(elements.Select(e => FoundationApi.Presentation.Translate(e.Label) + (FoundationApi.Kingdom != null && !FoundationApi.WorkspaceAccess.CanPurchase(e) ? " (locked)" : "")));
            choiceLabels.AddRange(Choices.Skip(elements.Count + 1).Select(id => id + " (unavailable)"));
        }

        internal void BuildContent(IModConfigContext context)
        {
            Begin();
            Dropdowns.Clear();
            KeyButtons.Clear();
            BuildHeader(context);
            BuildChoices();
            for (int slot = 0; slot < Pending.Length; slot++)
                BuildRow(context, slot);
            var row = context.Templates.CreateButtonRow(context.Container, "Defaults", "Reset All Shortcuts", ResetChoices, out _, out _);
            row.name = "ResetRow";
        }

        private void BuildHeader(IModConfigContext context)
        {
            if (FoundationApi.Kingdom != null && !HotkeyCustomizerPlugin.Unlocked)
            {
                context.Templates.CreateTextRow(context.Container, "Research Hotkey Customizer for free in Tech Tree > Mods.", false, out ResearchNotice);
                if (ResearchNotice)
                {
                    ResearchNotice.color = new Color(1f, .85f, .4f);
                    ResearchNotice.fontSize = 15;
                }
            }

            context.Templates.CreateToggleRow(context.Container, "Show Hotbar", PendingShowHotbar, value =>
            {
                CancelListening();
                PendingShowHotbar = value;
            }, out HotbarToggle);
            context.Templates.CreateTextRow(context.Container, "Custom Hotkeys", true, out _);
        }

        private void BuildRow(IModConfigContext context, int slot)
        {
            var row = context.Templates.CreateEmptyRow(context.Container, 54f);
            row.name = "BuildingRow_" + slot;
            var labelObject = new GameObject("SlotLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(row.transform, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = new Vector2(.20f, 1);
            labelRect.offsetMin = new Vector2(8, 0);
            labelRect.offsetMax = Vector2.zero;
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.fontSize = 16;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12;
            label.fontSizeMax = 18;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.text = "Building " + slot;
            BuildRowDropdown(context, row.transform, slot);
            BuildRowKeyButton(context, row.transform, slot);
        }

        private void BuildRowDropdown(IModConfigContext context, Transform parent, int slot)
        {
            context.Templates.CreateDropdownRow(parent, "", choiceLabels, Math.Max(0, Choices.IndexOf(Pending[slot] ?? "")), index =>
            {
                CancelListening();
                Pending[slot] = Choices[index];
            }, out var dropdown);
            dropdown.name = "BuildingDropdown_" + slot;
            var rect = (RectTransform)dropdown.transform;
            rect.anchorMin = new Vector2(.22f, .04f);
            rect.anchorMax = new Vector2(.68f, .96f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Dropdowns.Add(dropdown);
        }

        private void BuildRowKeyButton(IModConfigContext context, Transform parent, int slot)
        {
            context.Templates.CreateButtonRow(parent, "", BuildingShortcut.Parse(PendingKeys[slot]).Display, () => Listen(slot), out var button, out var keyLabel);
            button.name = "KeyButton_" + slot;
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(.70f, .04f);
            rect.anchorMax = new Vector2(.98f, .96f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            KeyButtons.Add(button);
            button.gameObject.AddComponent<NativeKeyHover>().Target = button.GetComponent<Image>();
            var nativeRow = FoundationApi.Bindings.Rows.ToArray().FirstOrDefault();
            if (nativeRow != null && keyLabel != null)
            {
                var nativeText = FoundationApi.Bindings.Text(nativeRow);
                if (nativeText != null)
                {
                    keyLabel.font = nativeText.font;
                    keyLabel.color = nativeText.color;
                }
            }

            if (keyLabel)
            {
                keyLabel.alignment = TextAlignmentOptions.Center;
                keyLabel.rectTransform.anchorMin = Vector2.zero;
                keyLabel.rectTransform.anchorMax = Vector2.one;
                keyLabel.rectTransform.offsetMin = keyLabel.rectTransform.offsetMax = Vector2.zero;
                keyColor = keyLabel.color;
            }
        }

        internal void Cancel()
        {
            Begin();
            RefreshUI();
        }

        internal void CancelListening()
        {
            if (Listening >= 0 && Listening < KeyButtons.Count && KeyButtons[Listening])
            {
                var label = KeyButtons[Listening].GetComponentInChildren<TMP_Text>();
                if (label)
                {
                    label.text = BuildingShortcut.Parse(PendingKeys[Listening]).Display;
                    label.color = keyColor != default ? keyColor : Color.white;
                }
            }

            if (nativeLabel)
            {
                nativeLabel.text = nativeText;
                nativeLabel.color = nativeColor;
            }

            ListeningAction = null;
            nativeLabel = null;
            Listening = -1;
        }

        internal void Commit()
        {
            var plugin = HotkeyCustomizerPlugin.Instance;
            if (plugin == null)
                return;
            bool saveOnSet = plugin.Config.SaveOnConfigSet;
            plugin.Config.SaveOnConfigSet = false;
            CancelListening();
            try
            {
                for (int slot = 0; slot < Pending.Length; slot++)
                {
                    plugin.Buildings[slot].Value = Pending[slot] ?? "";
                    plugin.Bindings[slot].Value = PendingKeys[slot] ?? "";
                }

                foreach (var name in PendingVanilla.Keys.ToArray())
                {
                    var action = FoundationApi.Bindings.Actions?.FindAction(name, false);
                    if (action != null)
                        VanillaBindings.Entry(action).Value = PendingVanilla[name];
                }

                plugin.ShowHotbar.Value = PendingShowHotbar;
            }
            finally
            {
                plugin.Config.SaveOnConfigSet = saveOnSet;
            }

            plugin.Config.Save();
        }

        internal readonly string[] Pending = new string[10];
        internal readonly string[] PendingKeys = new string[10];
        internal readonly List<TMP_Dropdown> Dropdowns = new List<TMP_Dropdown>();
        internal readonly List<Button> KeyButtons = new List<Button>();
        internal readonly Dictionary<string, string> PendingVanilla = new Dictionary<string, string>();
        internal bool PendingShowHotbar;
        internal Toggle HotbarToggle;
        internal TMP_Text ResearchNotice;
        internal InputAction ListeningAction;
        private TMP_Text nativeLabel;
        private string nativeText;
        private Color nativeColor;
        internal int Listening = -1;
        private int listenFrame;
        internal List<string> Choices;
        private List<string> choiceLabels;
        private Color keyColor;
        internal static BuildingSettings Instance { get; private set; }

        internal void Listen(int slot)
        {
            ModHotkeyCapture.Instance?.Cancel();
            CancelListening();
            Listening = slot;
            listenFrame = Time.frameCount;
            if (slot >= 0 && slot < KeyButtons.Count && KeyButtons[slot])
            {
                var label = KeyButtons[slot].GetComponentInChildren<TMP_Text>();
                if (label)
                {
                    keyColor = label.color;
                    label.text = "Press a key...";
                    label.color = Color.green;
                }
            }
        }

        internal void ListenNative(InputAction action, TMP_Text label)
        {
            ModHotkeyCapture.Instance?.Cancel();
            CancelListening();
            ListeningAction = action;
            nativeLabel = label;
            nativeText = label.text;
            nativeColor = label.color;
            listenFrame = Time.frameCount;
            label.text = "Press a key...";
            label.color = Color.green;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnDisable() => CancelListening();
        internal void RefreshUI()
        {
            if (HotbarToggle)
                HotbarToggle.SetIsOnWithoutNotify(PendingShowHotbar);
            for (int i = 0; i < Pending.Length; i++)
            {
                if (i < Dropdowns.Count && Dropdowns[i] != null && Choices != null)
                {
                    int index = Math.Max(0, Choices.IndexOf(Pending[i] ?? ""));
                    Dropdowns[i].SetValueWithoutNotify(index);
                    Dropdowns[i].RefreshShownValue();
                }

                if (i < KeyButtons.Count && KeyButtons[i] != null)
                {
                    var label = KeyButtons[i].GetComponentInChildren<TMP_Text>();
                    if (label)
                        label.text = BuildingShortcut.Parse(PendingKeys[i]).Display;
                }
            }
        }

        internal void RegisterModConfig()
        {
            var section = HotkeyCustomizerPlugin.Instance.SettingsSection();
            section.AddElement(new HotkeyConfigUIElement(this));
        }

        internal void ResetChoices()
        {
            CancelListening();
            PendingShowHotbar = true;
            for (int slot = 0; slot < Pending.Length; slot++)
            {
                Pending[slot] = "";
                PendingKeys[slot] = "";
            }

            foreach (var name in PendingVanilla.Keys.ToArray())
                PendingVanilla[name] = "";
            RefreshUI();
        }

        private void Update()
        {
            if ((Listening < 0 && ListeningAction == null) || Time.frameCount == listenFrame || Keyboard.current == null)
                return;
            var keyboard = Keyboard.current;
            foreach (var key in keyboard.allKeys)
            {
                if (!key.wasPressedThisFrame || BuildingShortcut.Modifier(key.keyCode))
                    continue;
                if (key.keyCode == Key.Escape)
                {
                    if (ModHotkeyCapture.Instance)
                        ModHotkeyCapture.Instance.EscapeFrame = Time.frameCount;
                    CancelListening();
                    return;
                }

                string value = key.keyCode == Key.Backspace || key.keyCode == Key.Delete ? "" : BuildingShortcut.Capture(keyboard, key.keyCode).ToString();
                if (ListeningAction != null)
                {
                    var action = ListeningAction;
                    var label = nativeLabel;
                    PendingVanilla[action.name] = value == "" ? VanillaBindings.Unassigned : value;
                    VanillaBindings.Apply(action, PendingVanilla[action.name]);
                    CancelListening();
                    label.text = BuildingShortcut.Parse(value).Display;
                    return;
                }

                PendingKeys[Listening] = value;
                CancelListening();
                return;
            }
        }
    }
}
