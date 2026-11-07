using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Flow;
using Assets.Source.Player;
using Assets.Source.Util;
using HarmonyLib;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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
        internal TMP_Text ResearchNotice;
        internal InputAction ListeningAction;
        TMP_Text nativeLabel;
        string nativeText;
        Color nativeColor;
        internal int Listening = -1;
        int listenFrame;
        internal List<string> Choices;
        Color keyColor;

        IModConfigSection configSection;
        internal static BuildingSettings Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            Begin();
            RegisterModConfig();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        internal void RegisterModConfig()
        {
            configSection = FoundationApi.ModConfig.GetOrCreateSection(HotkeyCustomizerPlugin.Id, "Hotkey Customizer", "Configure custom building shortcuts (slots 0-9) and the building hotbar.");
            configSection.ClearElements();

            // Custom Element for all Hotkey Customizer settings
            configSection.AddElement(new HotkeyConfigUIElement(this));
            configSection.OnCommit += Commit;
            configSection.OnCancel += Cancel;
        }

        internal void Begin()
        {
            CancelListening();
            var plugin = HotkeyCustomizerPlugin.Instance;
            PendingShowHotbar = plugin != null && plugin.ShowHotbar.Value;
            PendingVanilla.Clear();
            if (GameInput.RebindableKeys != null)
            {
                foreach (var action in GameInput.RebindableKeys)
                    PendingVanilla[action.name] = VanillaBindings.Entry(action).Value;
            }
            for (int digit = 0; digit < 10; digit++)
            {
                Pending[digit] = plugin != null ? plugin.Buildings[digit].Value : "";
                PendingKeys[digit] = plugin != null ? plugin.Bindings[digit].Value : "";
            }
        }

        internal void Commit()
        {
            var plugin = HotkeyCustomizerPlugin.Instance;
            if (plugin == null) return;

            bool saveOnSet = plugin.Config.SaveOnConfigSet;
            plugin.Config.SaveOnConfigSet = false;
            CancelListening();
            try
            {
                for (int digit = 0; digit < 10; digit++)
                {
                    plugin.Buildings[digit].Value = Pending[digit] ?? "";
                    plugin.Bindings[digit].Value = PendingKeys[digit] ?? "";
                }
                foreach (var action in PendingVanilla.Keys.ToArray())
                {
                    var act = GameInput.Asset?.FindAction(action, false);
                    if (act != null) VanillaBindings.Entry(act).Value = PendingVanilla[action];
                }
                plugin.ShowHotbar.Value = PendingShowHotbar;
            }
            finally
            {
                plugin.Config.SaveOnConfigSet = saveOnSet;
            }
            plugin.Config.Save();
        }

        internal void Cancel()
        {
            Begin();
            RefreshUI();
        }

        internal void ResetChoices()
        {
            CancelListening();
            PendingShowHotbar = true;
            for (int digit = 0; digit < 10; digit++)
            {
                Pending[digit] = "";
                PendingKeys[digit] = "";
            }
            foreach (var name in PendingVanilla.Keys.ToArray())
                PendingVanilla[name] = "";

            RefreshUI();
        }

        void OnDisable() => CancelListening();

        internal void Listen(int slot)
        {
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
            CancelListening();
            ListeningAction = action;
            nativeLabel = label;
            nativeText = label.text;
            nativeColor = label.color;
            listenFrame = Time.frameCount;
            label.text = "Press a key...";
            label.color = Color.green;
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
                    var action = ListeningAction;
                    var label = nativeLabel;
                    PendingVanilla[action.name] = value == "" ? VanillaBindings.Unassigned : value;
                    VanillaBindings.Apply(action, PendingVanilla[action.name]);
                    CancelListening();
                    label.text = BuildingShortcut.Parse(value).Display;
                    return;
                }
                PendingKeys[Listening] = value;
                if (Listening >= 0 && Listening < KeyButtons.Count && KeyButtons[Listening])
                {
                    var label = KeyButtons[Listening].GetComponentInChildren<TMP_Text>();
                    if (label)
                    {
                        label.text = BuildingShortcut.Parse(value).Display;
                        label.color = keyColor != default ? keyColor : Color.white;
                    }
                }
                CancelListening();
                return;
            }
        }

        internal void RefreshUI()
        {
            if (HotbarToggle) HotbarToggle.SetIsOnWithoutNotify(PendingShowHotbar);
            for (int i = 0; i < 10; i++)
            {
                if (i < Dropdowns.Count && Dropdowns[i] != null && Choices != null)
                {
                    int idx = Math.Max(0, Choices.IndexOf(Pending[i] ?? ""));
                    Dropdowns[i].SetValueWithoutNotify(idx);
                    Dropdowns[i].RefreshShownValue();
                }
                if (i < KeyButtons.Count && KeyButtons[i] != null)
                {
                    var label = KeyButtons[i].GetComponentInChildren<TMP_Text>();
                    if (label) label.text = BuildingShortcut.Parse(PendingKeys[i]).Display;
                }
            }
        }

        internal void BuildContent(IModConfigContext context)
        {
            Begin();
            CancelListening();
            Dropdowns.Clear();
            KeyButtons.Clear();

            // 1. Research notice if not unlocked
            if (GamePlayer.Current != null && !HotkeyCustomizerPlugin.Unlocked)
            {
                context.Templates.CreateTextRow(context.Container, "Research Hotkey Customizer for free in Tech Tree > Mods.", false, out ResearchNotice);
                if (ResearchNotice)
                {
                    ResearchNotice.color = new Color(1f, 0.85f, 0.4f);
                    ResearchNotice.fontSize = 15;
                }
            }

            // 2. Show Hotbar Toggle Row
            context.Templates.CreateToggleRow(context.Container, "Show Hotbar", PendingShowHotbar, val => {
                CancelListening();
                PendingShowHotbar = val;
            }, out HotbarToggle);

            // 3. Section Header for Shortcuts
            context.Templates.CreateTextRow(context.Container, "Custom Hotkeys", true, out SectionLabel);

            // 4. Catalog choices
            var elements = BuildingCatalog.All()
                .OrderBy(e => Translation.Translate(e.Label), StringComparer.CurrentCultureIgnoreCase).ToList();
            Choices = new List<string> { "" };
            Choices.AddRange(elements.Select(e => e.ElementType));
            Choices.AddRange(Pending.Where(id => !string.IsNullOrEmpty(id) && !Choices.Contains(id)).Distinct());

            var labels = new List<string> { "Unassigned" };
            labels.AddRange(elements.Select(e => Translation.Translate(e.Label)
                + (GamePlayer.Current != null && !e.IsPurchaseable(GamePlayer.Current) ? " (locked)" : "")));
            labels.AddRange(Choices.Skip(elements.Count + 1).Select(id => id + " (unavailable)"));

            // 5. Ten Building Rows (Building Dropdown + Key Button)
            for (int digit = 0; digit < 10; digit++)
            {
                int slot = digit;
                var row = context.Templates.CreateEmptyRow(context.Container, 54f);
                row.name = "BuildingRow_" + slot;

                // Slot Label
                var labelObj = new GameObject("SlotLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(row.transform, false);
                var lRect = (RectTransform)labelObj.transform;
                lRect.anchorMin = new Vector2(0, 0);
                lRect.anchorMax = new Vector2(0.20f, 1);
                lRect.offsetMin = new Vector2(8, 0);
                lRect.offsetMax = Vector2.zero;

                var lText = labelObj.GetComponent<TextMeshProUGUI>();
                lText.fontSize = 16;
                lText.enableAutoSizing = true;
                lText.fontSizeMin = 12;
                lText.fontSizeMax = 18;
                lText.alignment = TextAlignmentOptions.MidlineLeft;
                lText.text = "Building " + slot;

                // Dropdown in middle
                context.Templates.CreateDropdownRow(row.transform, "", labels, Math.Max(0, Choices.IndexOf(Pending[slot] ?? "")), idx => {
                    CancelListening();
                    Pending[slot] = Choices[idx];
                }, out var dropdown);
                dropdown.name = "BuildingDropdown_" + slot;
                var dRect = (RectTransform)dropdown.transform;
                dRect.anchorMin = new Vector2(0.22f, 0.04f);
                dRect.anchorMax = new Vector2(0.68f, 0.96f);
                dRect.offsetMin = dRect.offsetMax = Vector2.zero;
                Dropdowns.Add(dropdown);

                // Key Button on right
                context.Templates.CreateButtonRow(row.transform, "", BuildingShortcut.Parse(PendingKeys[slot]).Display, () => Listen(slot), out var keyBtn, out var keyLabel);
                keyBtn.name = "KeyButton_" + slot;
                var bRect = (RectTransform)keyBtn.transform;
                bRect.anchorMin = new Vector2(0.70f, 0.04f);
                bRect.anchorMax = new Vector2(0.98f, 0.96f);
                bRect.offsetMin = bRect.offsetMax = Vector2.zero;
                KeyButtons.Add(keyBtn);
                keyBtn.gameObject.AddComponent<NativeKeyHover>().Target = keyBtn.GetComponent<Image>();
                var kbRow = Resources.FindObjectsOfTypeAll<KeyBindingRow>().FirstOrDefault();
                if (kbRow != null && keyLabel != null)
                {
                    var nativeBindingText = (TMP_Text)AccessTools.Field(typeof(KeyBindingRow), "_binding")?.GetValue(kbRow);
                    if (nativeBindingText != null)
                    {
                        keyLabel.font = nativeBindingText.font;
                        keyLabel.color = nativeBindingText.color;
                    }
                }
                if (keyLabel)
                {
                    keyLabel.alignment = TextAlignmentOptions.Center;
                    var klRect = keyLabel.rectTransform;
                    klRect.anchorMin = Vector2.zero;
                    klRect.anchorMax = Vector2.one;
                    klRect.offsetMin = klRect.offsetMax = Vector2.zero;
                    keyColor = keyLabel.color;
                }
            }

            // 6. Reset button
            var resetRow = context.Templates.CreateButtonRow(context.Container, "Defaults", "Reset All Shortcuts", () => {
                ResetChoices();
            }, out _, out _);
            resetRow.name = "ResetRow";
        }

        sealed class HotkeyConfigUIElement : IModConfigElement
        {
            public string Id => "urakka.hotkeys.settings";
            public bool IsVisible { get; set; } = true;
            readonly BuildingSettings settings;

            public HotkeyConfigUIElement(BuildingSettings settings)
            {
                this.settings = settings;
            }

            public GameObject CreateView(IModConfigContext context)
            {
                var container = new GameObject("HotkeyCustomizerControls", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                container.transform.SetParent(context.Container, false);
                var vlg = container.GetComponent<VerticalLayoutGroup>();
                vlg.spacing = 8;
                vlg.childControlWidth = true;
                vlg.childControlHeight = false;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                var csf = container.GetComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var subContext = new SubContext((RectTransform)container.transform, context.Templates);
                settings.BuildContent(subContext);
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)container.transform);
                return container;
            }

            public void Commit() => settings.Commit();
            public void Cancel() => settings.Cancel();

            sealed class SubContext : IModConfigContext
            {
                public RectTransform Container { get; }
                public IModConfigTemplates Templates { get; }
                public SubContext(RectTransform c, IModConfigTemplates t) { Container = c; Templates = t; }
            }
        }
    }
}
