using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Flow;
using Assets.Source.Player;
using Assets.Source.Util;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LightJson;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class HotkeyCustomizerPlugin
    {
        readonly List<string> checks = new List<string>();
        readonly List<string> errors = new List<string>();
        void Check(bool pass, string name) { if (!pass) throw new InvalidOperationException(name); checks.Add(name); }
        static void Keys(Keyboard keyboard, params Key[] keys)
        { InputSystem.EnableDevice(keyboard); keyboard.MakeCurrent(); InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); InputSystem.Update(); }
        static string HashFile(string path)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        void WatchErrors(string message, string stack, LogType type)
        { if (type == LogType.Exception || type == LogType.Error) errors.Add(message + "\n" + stack); }

        IEnumerator RuntimeCheck()
        {
            Application.logMessageReceived += WatchErrors;
            yield return new WaitForSecondsRealtime(5);
            string directory = Path.Combine(Paths.GameRootPath, "_Mods", "HotkeyCustomizer", "Verification");
            Directory.CreateDirectory(directory);
            string error = null;
            Keyboard keyboard = null; Mouse mouse = null;
            KeyBindingsUI menu = null; BuildingSettings settings = null;
            BuildingHotbar hotbar = null;
            string original = null;
            OptionsUI options = null;
            try
            {
                Check(GamePlayer.Current == null && !GameUI.Instance, "No normal kingdom loaded");
                Check(!SaveGame.LoadLatestSave(), "Diagnostic blocks save loading");
                Check(Bindings.All(b => b.Value == "") && Buildings.All(b => b.Value == ""), "Fresh installation has ten blank keys and building choices");
                original = GameInput.Asset.SaveBindingOverridesAsJson();
                GameInput.Asset.FindAction("Hotkey2", true).ApplyBindingOverride(0, "<Keyboard>/f8");
                string customized = GameInput.Asset.SaveBindingOverridesAsJson();
                MainMenuUI.Instance.ShowOptions();
                options = Resources.FindObjectsOfTypeAll<OptionsUI>().First(o => o.gameObject.activeInHierarchy);
                var configMenu = options.GetComponent<KingdomFoundation.FoundationConfigMenu>() ?? KingdomFoundation.FoundationConfigMenu.Attach(options);
                configMenu.ShowModsTab(Id);
                settings = BuildingSettings.Instance;
                Check(GameInput.Asset.SaveBindingOverridesAsJson() == customized, "Opening mod settings preserves existing custom workspace bindings");
                Check(settings.Dropdowns.Count == 10 && settings.KeyButtons.Count == 10, "Main menu displays ten key buttons and ten building dropdowns");
                Check(settings.KeyButtons.All(b => b.GetComponentInChildren<TMP_Text>().text == "Unassigned"), "All ten key buttons start Unassigned");
                Check(settings.SectionLabel.text == "Custom Hotkeys" && settings.HotbarToggle.isOn && ShowHotbar.Value,
                    "Custom Hotkeys section includes a native Show Hotbar checkbox enabled by default");
                settings.HotbarToggle.isOn = false;
                Check(!settings.PendingShowHotbar && ShowHotbar.Value, "Hotbar checkbox remains pending until Apply");
                Check(settings.Choices.Contains("Mine") && settings.Choices.Contains("Manifold"), "Dropdown contains real building types");
                Check(BuildingCatalog.All().All(e => e.RequiredTech != null), "Dropdown catalog contains only research-backed purchasable types");
                Check(GamePlayer.Current == null, "Main-menu catalog restores the empty player context without loading a kingdom");
                settings.Dropdowns[0].value = settings.Choices.IndexOf("Mine");
                Check(settings.Pending[0] == "Mine" && Buildings[0].Value == "", "Dropdown edits remain pending until Apply");
                settings.PendingKeys[0] = "Ctrl+F6";
                settings.Commit();
                Check(!ShowHotbar.Value && new ConfigFile(Config.ConfigFilePath, false).Bind("Interface", "ShowHotbar", true).Value == false,
                    "Show Hotbar preference is saved and reloads");
                Check(Buildings[0].Value == "Mine" && Bindings[0].Value == "Ctrl+F6", "Apply commits building and key together");
                var reload = new ConfigFile(Config.ConfigFilePath, false);
                Check(reload.Bind("Buildings", "Slot0", "").Value == "Mine" && reload.Bind("Hotkeys", "Slot0", "").Value == "Ctrl+F6", "Assignments persist in the mod configuration across reload");
                Check(GameInput.Asset.SaveBindingOverridesAsJson() == customized, "Applying mod assignments leaves native workspace bindings unchanged");
                settings.Begin(); settings.Pending[0] = "Manifold"; settings.PendingKeys[0] = "Alt+F7";
                options.CancelOptions();
                MainMenuUI.Instance.ShowOptions();
                configMenu.ShowModsTab(Id);
                Check(!settings.HotbarToggle.isOn, "Opening settings restores the saved visibility preference");
                settings.HotbarToggle.isOn = true;
                options.CancelOptions();
                MainMenuUI.Instance.ShowOptions();
                configMenu.ShowModsTab(Id);
                Check(!settings.PendingShowHotbar && !ShowHotbar.Value, "Cancel discards hotbar checkbox edits");
                Check(settings.Pending[0] == "Mine" && settings.PendingKeys[0] == "Ctrl+F6", "Cancel discards pending building and key edits");
                settings.ResetChoices();
                Check(settings.Pending.All(s => s == "") && settings.PendingKeys.All(s => s == ""), "Reset clears all mod entries in the pending form");
                Check(settings.HotbarToggle.isOn && !ShowHotbar.Value, "Reset restores Show Hotbar in the pending form only");
                Check(Buildings[0].Value == "Mine" && Bindings[0].Value == "Ctrl+F6", "Reset remains reversible until Apply");
                keyboard = InputSystem.AddDevice<Keyboard>(); keyboard.MakeCurrent();
                mouse = InputSystem.AddDevice<Mouse>(); mouse.MakeCurrent();
                Keys(keyboard);
                settings.Listen(1);
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return null;
            if (error == null) try
            {
                Keys(keyboard, Key.RightCtrl, Key.F7);
                AccessTools.Method(typeof(BuildingSettings), "Update").Invoke(settings, null);
                Check(settings.PendingKeys[1] == "Ctrl+F7" && settings.Listening == -1, "Rebind button captures right Ctrl plus a key (" + settings.PendingKeys[1] + "; listening=" + settings.Listening + "; ctrl=" + Keyboard.current.ctrlKey.isPressed + ")");
                Check(Bindings[1].Value == "", "Captured key is pending until Apply");
                Keys(keyboard);
                settings.Listen(1);
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return null;
            if (error == null) try
            {
                Keys(keyboard, Key.Backspace);
                AccessTools.Method(typeof(BuildingSettings), "Update").Invoke(settings, null);
                Check(settings.PendingKeys[1] == "", "Backspace clears an assigned shortcut");
                Keys(keyboard);
                settings.Begin();
            }
            catch (Exception ex) { error = ex.ToString(); }
            // Newly cloned native dropdowns initialize their tween runner in Start.
            yield return null;
            if (error == null) try
            {
                settings.Dropdowns[0].Show();
                Check(settings.Dropdowns[0].IsExpanded, "Native building dropdown opens");
                Check(Resources.FindObjectsOfTypeAll<Toggle>().Any(t => t.gameObject.activeInHierarchy && t.GetComponentInChildren<TMP_Text>()?.text == "Mine"), "Expanded dropdown contains selectable building entries");
                settings.Dropdowns[0].Hide();
                Check(BuildingShortcut.Parse("Ctrl+Alt+Shift+F6").ToString() == "Ctrl+Alt+Shift+F6", "Multiple modifiers round trip");
                Check(BuildingShortcut.Parse("bogus").Key == Key.None && BuildingShortcut.Parse("Ctrl+LeftCtrl").Key == Key.None, "Invalid or modifier-only config remains unassigned");
                Check(((RectTransform)settings.Dropdowns[0].transform).anchorMax.x <= ((RectTransform)settings.KeyButtons[0].transform).anchorMin.x,
                    "Building dropdown is left of the key setter");
                Capture(Path.Combine(directory, "hotkey-customizer-settings.png"));
                options.CancelOptions();
                GameInput.Asset.LoadBindingOverridesFromJson(original);
                MainMenuUI.Instance.ShowKeyBindings();
                menu = Resources.FindObjectsOfTypeAll<KeyBindingsUI>().First(k => k.gameObject.activeInHierarchy);
                AccessTools.Method(typeof(KeyBindingsUI), "Start").Invoke(menu, null);
                menu.RefreshBindings();
                ScrollToBottom(menu);
                var prefab = (KeyBindingRow)AccessTools.Field(typeof(KeyBindingsUI), "_rowPrefab").GetValue(menu);
                var nativeText = (TMP_Text)AccessTools.Field(typeof(KeyBindingRow), "_binding").GetValue(prefab);
                Check(settings.KeyButtons[0].GetComponentInChildren<TMP_Text>().font == nativeText.font
                    && settings.KeyButtons[0].GetComponentInChildren<TMP_Text>().color == nativeText.color,
                    "Custom key setters retain vanilla font and text color");
                Check(settings.KeyButtons[0].GetComponent<NativeKeyHover>(), "Custom key setter uses vanilla hover colors");
                CaptureNative(menu, "Hotkey1", keyboard, Key.RightCtrl, Key.RightShift, Key.Digit1);
                Check(settings.PendingVanilla["Hotkey1"] == "Ctrl+Shift+Digit1", "Vanilla row captures multiple modifiers and final key");
                Check(VanillaBindings.Entry(GameInput.Controls.UI.Hotkey1).Value == "", "Vanilla capture remains pending until Apply");
                menu.ApplyBindings();
                Check(VanillaBindings.Entry(GameInput.Controls.UI.Hotkey1).Value == "Ctrl+Shift+Digit1", "Apply saves vanilla combination in mod configuration");
                Check(!MemoryPrefs.GetString("InputBindingOverrides").Contains(VanillaBindings.Interaction), "Native saved preferences contain no mod-specific interaction");
                KeyBindingsUI.Apply();
                Check(GameInput.Controls.UI.Hotkey1.bindings[0].overrideInteractions.Contains(VanillaBindings.Interaction), "Reload restores runtime modifier gate");
                var reloadedConfig = new ConfigFile(Config.ConfigFilePath, false);
                Check(reloadedConfig.Bind("VanillaHotkeys", "Hotkey1", "").Value == "Ctrl+Shift+Digit1", "Vanilla chord persists across config reload");
                MainMenuUI.Instance.ShowKeyBindings();
                CaptureNative(menu, "Hotkey1", keyboard, Key.LeftAlt, Key.F10);
                menu.CancelBindings();
                Check(VanillaBindings.Entry(GameInput.Controls.UI.Hotkey1).Value == "Ctrl+Shift+Digit1"
                    && GameInput.Controls.UI.Hotkey1.bindings[0].effectivePath == "<Keyboard>/1", "Cancel restores saved vanilla chord and base key");
                MainMenuUI.Instance.ShowKeyBindings(); menu.ResetBindings();
                Check(string.IsNullOrEmpty(GameInput.Controls.UI.Hotkey1.bindings[0].overrideInteractions), "Reset removes the pending vanilla modifier gate");
                menu.CancelBindings();
                Check(GameInput.Controls.UI.Hotkey1.bindings[0].overrideInteractions.Contains(VanillaBindings.Interaction), "Cancel after Reset restores vanilla combination");
                MainMenuUI.Instance.ShowKeyBindings();
                CaptureNative(menu, "TogglePurchaseMenu", keyboard, Key.LeftCtrl, Key.B);
                CaptureNative(menu, "Hotkey9", keyboard, Key.Delete);
                menu.ApplyBindings();
                Check(GameInput.Controls.UI.Hotkey9.bindings[0].effectivePath == "", "Vanilla shortcut can be cleared");
                MainMenuUI.Instance.ShowKeyBindings();
                CaptureNative(menu, "Hotkey9", keyboard, Key.Digit9); menu.ApplyBindings();
                Check(VanillaBindings.Entry(GameInput.Controls.UI.Hotkey9).Value == "Digit9", "Vanilla single-key assignment still works");
                menu.CancelBindings();
                MainMenuUI.Instance.ShowMainMenu();
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.4f);
            if (error == null) try
            {
                var player = GamePlayer.CreateNewPlayer();
                player.AddTech("mine"); player.AddTech("manifold");
                player.AddCurrency(Currency.Gold, 1000000000, false); player.AddCurrency(Currency.Medal, 1000000000, false);
                player.ActiveFlow.Elements.Clear(); player.ActiveFlow.Pipes.Clear(); player.ActiveFlow.Decorations.Clear();
                player.ActiveFlow.Label = "Hotkey Customizer disposable verification";
                for (int i = 1; i < 10; i++) player.Flows.Add(new FlowDiagram { Label = "Workspace " + (i + 1) });
                SceneManager.LoadScene("Game");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(4);
            if (error == null) try
            {
                Keys(keyboard); EventSystem.current.SetSelectedGameObject(null);
                hotbar = GameUI.Instance.GetComponent<BuildingHotbar>();
                VerifyResearchLocked(hotbar, keyboard);
                OpenResearch();
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.4f);
            if (error == null) try
            {
                Capture(Path.Combine(directory, "hotkey-customizer-tech.png"));
                VerifyResearchPurchase();
                GameUI.Instance.ToggleTechMenu(false);
                Keys(keyboard); EventSystem.current.SetSelectedGameObject(null);
                Check(CanUseBuildings(), "Gameplay permits building hotkeys");
                Check(hotbar && hotbar.Items.Length == 10 && !hotbar.Strip.gameObject.activeSelf, "Hidden preference hides the gameplay hotbar");
                Keys(keyboard, Key.F6); Update();
                Check(!WorldFlowManager.Instance.BlueprintActive, "Plain key does not trigger a Ctrl shortcut");
                Keys(keyboard); Keys(keyboard, Key.LeftCtrl, Key.F6); Update();
                Check(WorldFlowManager.Instance.BlueprintActive && CurrentGhostType() == "Mine", "Assigned Ctrl hotkey opens the normal Mine placement preview");
                Check(!ShowHotbar.Value, "Keyboard shortcut remains active with Show Hotbar disabled");
                int before = GamePlayer.Current.ActiveFlow.Elements.Count;
                Check(before == 0, "Shortcut does not place or charge for a building directly");
                EndPreview();
                Keys(keyboard); Keys(keyboard, Key.LeftCtrl, Key.LeftShift, Key.F6); Update();
                Check(!WorldFlowManager.Instance.BlueprintActive, "Extra modifier does not trigger a shortcut");
                Keys(keyboard); Keys(keyboard, Key.RightCtrl, Key.F6); Update();
                Check(WorldFlowManager.Instance.BlueprintActive, "Right Ctrl triggers assigned Ctrl shortcut");
                EndPreview();
                Keys(keyboard); Keys(keyboard, Key.Digit2);
                Check(GamePlayer.Current.CurrentFlow == 1, "Native plain 2 still opens workspace page 2");
                Keys(keyboard); Keys(keyboard, Key.Digit0);
                Check(GamePlayer.Current.CurrentFlow == 9, "Native plain 0 still opens workspace page 10");
                Keys(keyboard); WorldFlowManager.Instance.ShowFlow(0);
                WorldFlowManager.Instance.ShowFlow(2);
                Keys(keyboard, Key.Digit1);
                Check(GamePlayer.Current.CurrentFlow == 2, "Vanilla workspace combination ignores its plain number key");
                Keys(keyboard); Keys(keyboard, Key.LeftCtrl, Key.Digit1);
                Check(GamePlayer.Current.CurrentFlow == 2, "Vanilla workspace combination requires every modifier");
                Keys(keyboard); Keys(keyboard, Key.LeftCtrl, Key.LeftShift, Key.Digit1);
                Check(GamePlayer.Current.CurrentFlow == 0, "Vanilla Ctrl + Shift + 1 invokes the native workspace action");
                WorldFlowManager.Instance.ShowFlow(2);
                Keys(keyboard); Keys(keyboard, Key.RightCtrl, Key.RightShift, Key.Digit1);
                Check(GamePlayer.Current.CurrentFlow == 0, "Vanilla combination accepts right-side modifiers");
                WorldFlowManager.Instance.ShowFlow(2);
                Keys(keyboard); Keys(keyboard, Key.LeftCtrl, Key.LeftShift, Key.LeftAlt, Key.Digit1);
                Check(GamePlayer.Current.CurrentFlow == 2, "Extra modifier does not trigger vanilla combination");
                Keys(keyboard); Keys(keyboard, Key.B);
                Check(!GameUI.Instance.IsActive(GameUIType.BuyMenu), "Vanilla non-workspace action ignores its unmodified key");
                Keys(keyboard); Keys(keyboard, Key.LeftCtrl, Key.B);
                Check(GameUI.Instance.IsActive(GameUIType.BuyMenu), "Vanilla Ctrl + B invokes the native purchase-menu callback");
                GameUI.Instance.ToggleBuyMenu(false); Keys(keyboard); WorldFlowManager.Instance.ShowFlow(0);
                Bindings[1].Value = "Alt+F7"; Buildings[1].Value = "Manifold";
                ShowHotbar.Value = true; hotbar.Refresh();
                Check(hotbar.Strip.gameObject.activeSelf && Mathf.Approximately(hotbar.Strip.anchoredPosition.x, hotbar.Strip.rect.width - BuildingHotbar.Peek),
                    "Enabled hotbar starts collapsed with an edge visible");
                Check(hotbar.Items[0].Icon.sprite == FlowElement.GetAllTypes().First(e => e.ElementType == "Mine").Icon
                    && hotbar.Items[0].GetTooltipTitle() == Translation.Translate(FlowElement.GetAllTypes().First(e => e.ElementType == "Mine").Label),
                    "Hotbar uses the assigned building icon and hover title");
                Check(hotbar.Items[0].GetComponentsInChildren<TMP_Text>().All(t => t == hotbar.Items[0].KeyLabel),
                    "Hotbar tiles contain shortcut labels and no visible building names");
                Capture(Path.Combine(directory, "hotbar-collapsed.png"));
                hotbar.Strip.GetComponent<HotbarHover>().OnPointerEnter(new PointerEventData(EventSystem.current));
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                Check(Mathf.Approximately(hotbar.Strip.anchoredPosition.x, 0), "Hover fully reveals the vertical hotbar");
                var source = hotbar.Items[0].GetComponentInChildren<Behaviour.UI.Tooltip.TooltipSource>();
                Check(source && source.GetTitle() == hotbar.Items[0].GetTooltipTitle(), "Native hover source resolves the assigned building name");
                source.OnPointerEnter(new PointerEventData(EventSystem.current));
                Check(Behaviour.UI.UITooltip.Active && FindObjectsByType<Behaviour.UI.UITooltip>(FindObjectsSortMode.None)
                    .Any(t => t.Source == source && t.GetComponentsInChildren<TMP_Text>().Any(label => label.text == source.GetTitle())),
                    "Hover displays the building name in the native tooltip");
                Capture(Path.Combine(directory, "hotbar-tooltip.png"));
                source.OnPointerExit(new PointerEventData(EventSystem.current));
                Capture(Path.Combine(directory, "hotbar-expanded.png"));
                hotbar.Items[0].OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
                Check(CurrentGhostType() == "Mine", "Clicking a hotbar icon opens normal building placement");
                EndPreview();
                hotbar.Strip.GetComponent<HotbarHover>().OnPointerExit(new PointerEventData(EventSystem.current));
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.4f);
            if (error == null) try
            {
                Check(Mathf.Approximately(hotbar.Strip.anchoredPosition.x, hotbar.Strip.rect.width - BuildingHotbar.Peek), "Leaving the hotbar returns it to the edge");
                Keys(keyboard, Key.LeftAlt, Key.F7); Update();
                Check(CurrentGhostType() == "Manifold", "A second entry selects its own assigned building");
                EndPreview(); Keys(keyboard);
                var locked = FlowElement.GetAllTypes().First(e => e.RequiredTech != null && !e.IsPurchaseable(GamePlayer.Current));
                Buildings[2].Value = locked.ElementType;
                hotbar.Refresh(); hotbar.Items[2].OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
                Check(!WorldFlowManager.Instance.BlueprintActive && hotbar.Items[2].Icon.color.a < .5f,
                    "Hotbar dims locked buildings and cannot bypass their research");
                Check(!SelectBuilding(2) && !WorldFlowManager.Instance.BlueprintActive, "Locked research cannot be bypassed by a hotkey");
                Buildings[2].Value = "MissingCompanionBuilding";
                Check(!SelectBuilding(2), "Unavailable building IDs safely do nothing");
                Buildings[2].Value = "Manifold";
                foreach (Currency currency in Enum.GetValues(typeof(Currency)))
                    GamePlayer.Current.AddCurrency(currency, -GamePlayer.Current.GetCurrency(currency), false);
                Check(!SelectBuilding(2) && !WorldFlowManager.Instance.BlueprintActive, "Insufficient funds cannot be bypassed");
                GamePlayer.Current.AddCurrency(Currency.Gold, 1000000000, false); GamePlayer.Current.AddCurrency(Currency.Medal, 1000000000, false);
                GameInput.Disable(); Check(!SelectBuilding(0), "Disabled gameplay input blocks shortcuts"); GameInput.Enable();
                Time.timeScale = 0; Check(!SelectBuilding(0), "Pause blocks shortcuts"); Time.timeScale = 1;
                GameUI.Instance.ToggleBuyMenu(true);
                var buy = (Assets.Behaviour.UI.BuyMenu.ElementBuyMenu)AccessTools.Field(typeof(GameUI), "_buyMenu").GetValue(GameUI.Instance);
                var searchBar = (RectTransform)AccessTools.Field(typeof(Assets.Behaviour.UI.BuyMenu.ElementBuyMenu), "_searchBar").GetValue(buy);
                if (!searchBar.gameObject.activeSelf) buy.ToggleSearch();
                var input = (TMP_InputField)AccessTools.Field(typeof(Assets.Behaviour.UI.BuyMenu.ElementBuyMenu), "_searchInput").GetValue(buy);
                EventSystem.current.SetSelectedGameObject(input.gameObject);
                Check(!SelectBuilding(0), "Text entry blocks building shortcuts");
                input.DeactivateInputField(); buy.ToggleSearch(); GameUI.Instance.ToggleBuyMenu(false);
                EventSystem.current.SetSelectedGameObject(null);
                var ingame = Resources.FindObjectsOfTypeAll<InGameMenuUI>().First();
                ingame.gameObject.SetActive(true); ingame.ShowOptions();
                var ingameOptions = Resources.FindObjectsOfTypeAll<OptionsUI>().First(o => o.gameObject.activeInHierarchy);
                var ingameConfigMenu = ingameOptions.GetComponent<KingdomFoundation.FoundationConfigMenu>() ?? KingdomFoundation.FoundationConfigMenu.Attach(ingameOptions);
                ingameConfigMenu.ShowModsTab(Id);
                var ingameSettings = SettingsFor(null);
                hotbar.Refresh();
                Check(!hotbar.Strip.gameObject.activeSelf, "Hotbar hides behind the pause settings");
                Check(ingameSettings.Dropdowns.Count == 10 && ingameSettings.PendingKeys[0] == "Ctrl+F6", "Pause menu shares persisted building and key assignments");
                Check(ingameSettings.SectionLabel && ingameSettings.HotbarToggle.isOn, "Pause settings include Custom Hotkeys and the saved Show Hotbar checkbox");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(.3f);
            if (error == null) try
            {
                Capture(Path.Combine(directory, "hotkey-customizer-ingame-settings.png"));
                var ingame = Resources.FindObjectsOfTypeAll<InGameMenuUI>().First();
                var ingameOptions = Resources.FindObjectsOfTypeAll<OptionsUI>().First(o => o.gameObject.activeInHierarchy);
                var ingameConfigMenu = ingameOptions.GetComponent<KingdomFoundation.FoundationConfigMenu>() ?? KingdomFoundation.FoundationConfigMenu.Attach(ingameOptions);
                var ingameSettings = SettingsFor(null);
                ingameSettings.PendingKeys[0] = "Shift+F9";
                ingameOptions.ApplyAndClose();
                Check(Bindings[0].Value == "Shift+F9", "Native Apply button commits the mod's key assignment");
                ingame.ShowOptions();
                ingameConfigMenu.ShowModsTab(Id);
                ingameSettings.Listen(0);
                ingameSettings.CancelListening();
                Check(Bindings[0].Value == "Shift+F9", "Canceling capture preserves the saved key");
                ingameOptions.CancelOptions();
                ingame.DoResume();
                harmony.UnpatchSelf();
                Check(!Harmony.GetPatchInfo(AccessTools.Method(typeof(GameUI), nameof(GameUI.DoHotkey)))?.Owners.Contains(Id) ?? true,
                    "No native workspace action was patched by the mod");
                Check(errors.Count == 0, "Unity produced no errors during runtime/UI checks");
            }
            catch (Exception ex) { error = ex.ToString(); }
            Time.timeScale = 1;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            var report = new JsonObject {
                {"passed", error == null}, {"version", Version}, {"gameVersion", Application.version},
                {"unityVersion", Application.unityVersion}, {"bepInExVersion", typeof(BaseUnityPlugin).Assembly.GetName().Version.ToString()},
                {"dllSha256", HashFile(typeof(HotkeyCustomizerPlugin).Assembly.Location)}, {"count", checks.Count},
                {"checks", new JsonArray(checks.Select(c => (JsonValue)c).ToArray())},
                {"plugins", new JsonArray(BepInEx.Bootstrap.Chainloader.PluginInfos.Values.Select(p => (JsonValue)(p.Metadata.Name + " " + p.Metadata.Version)).ToArray())},
                {"unityErrors", new JsonArray(errors.Select(c => (JsonValue)c).ToArray())}, {"error", error ?? ""}
            };
            File.WriteAllText(Path.Combine(directory, "runtime-tests.json"), report.ToString());
            Application.logMessageReceived -= WatchErrors;
            if (error != null) Logger.LogError(error); else Logger.LogInfo("Hotkey Customizer diagnostic passed: " + checks.Count + " checks.");
            Application.Quit();
        }

        static void EndPreview()
        {
            WorldFlowManager.Instance.StopBlueprintMode();
            // Destroy is deferred until end of frame. Tests simulate several key
            // transitions synchronously, so clear the reference as the next frame would.
            AccessTools.Field(typeof(WorldFlowManager), "_currentGhost").SetValue(WorldFlowManager.Instance, null);
        }
        static void CaptureNative(KeyBindingsUI menu, string actionName, Keyboard keyboard, params Key[] keys)
        {
            var row = menu.GetComponentsInChildren<KeyBindingRow>().First(r =>
                ((InputAction)AccessTools.Field(typeof(KeyBindingRow), "_bindingRef").GetValue(r)).name == actionName);
            row.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            var settings = SettingsFor(menu);
            // Simulate a subsequent key event without waiting an extra frame per row.
            AccessTools.Field(typeof(BuildingSettings), "listenFrame").SetValue(settings, -1);
            Keys(keyboard); Keys(keyboard, keys);
            AccessTools.Method(typeof(BuildingSettings), "Update").Invoke(settings, null);
        }
        static string CurrentGhostType()
        {
            var ghost = (WorldFlowGhost)AccessTools.Field(typeof(WorldFlowManager), "_currentGhost").GetValue(WorldFlowManager.Instance);
            return ghost ? ((FlowElement)AccessTools.Field(typeof(WorldFlowGhost), "_element").GetValue(ghost))?.ElementType : null;
        }

        static void ScrollToBottom(KeyBindingsUI menu)
        {
            Canvas.ForceUpdateCanvases();
            var parent = (RectTransform)AccessTools.Field(typeof(KeyBindingsUI), "_rowParent").GetValue(menu);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            var scroll = parent.GetComponentInParent<ScrollRect>();
            if (scroll) scroll.verticalNormalizedPosition = 0;
            Canvas.ForceUpdateCanvases();
        }

        static void ScrollToCustomSection(KeyBindingsUI menu, BuildingSettings settings)
        {
            Canvas.ForceUpdateCanvases();
            var parent = (RectTransform)AccessTools.Field(typeof(KeyBindingsUI), "_rowParent").GetValue(menu);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            var scroll = parent.GetComponentInParent<ScrollRect>();
            var heading = (RectTransform)settings.SectionLabel.transform.parent;
            float offset = parent.rect.yMax - parent.InverseTransformPoint(heading.TransformPoint(new Vector3(0, heading.rect.yMax, 0))).y;
            scroll.verticalNormalizedPosition = 1 - Mathf.Clamp01(offset / (parent.rect.height - scroll.viewport.rect.height));
            Canvas.ForceUpdateCanvases();
        }

        static void Capture(string path)
        {
            // Overlay canvases do not appear in Camera.Render. Temporarily render
            // those existing canvases through the game camera for hidden-player QA.
            Canvas.ForceUpdateCanvases();
            var camera = Camera.main;
            var overlays = FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            var cameras = overlays.Select(c => c.worldCamera).ToArray();
            var distances = overlays.Select(c => c.planeDistance).ToArray();
            var orders = overlays.Select(c => c.sortingOrder).ToArray();
            var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
            var previous = RenderTexture.active; var oldTarget = camera.targetTexture; int oldMask = camera.cullingMask;
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            try
            {
                foreach (var canvas in overlays) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 10; canvas.sortingOrder = 30000; }
                camera.cullingMask = -1; camera.targetTexture = target;
                Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target; texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); texture.Apply();
                File.WriteAllBytes(path, ImageConversion.EncodeToPNG(texture));
            }
            finally
            {
                camera.targetTexture = oldTarget; camera.cullingMask = oldMask;
                for (int i = 0; i < overlays.Length; i++) { overlays[i].renderMode = RenderMode.ScreenSpaceOverlay; overlays[i].worldCamera = cameras[i]; overlays[i].planeDistance = distances[i]; overlays[i].sortingOrder = orders[i]; }
                Canvas.ForceUpdateCanvases();
                RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); Destroy(texture);
            }
        }
        void InstallDiagnostics()
        {
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            var safety = new Harmony(Id + ".diagnostics");
            foreach (var type in new[] { typeof(NoSave), typeof(NoAutosave), typeof(NoLoad), typeof(NoAchievement), typeof(Offline), typeof(IsolatedBindings) })
                safety.CreateClassProcessor(type).Patch();
        }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.Store))] static class NoSave { static bool Prefix() => false; }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.StoreAutosaveState))] static class NoAutosave { static bool Prefix() => false; }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.LoadLatestSave))] static class NoLoad { static bool Prefix(ref bool __result) { __result = false; return false; } }
        [HarmonyPatch(typeof(SteamAchievement), nameof(SteamAchievement.Trigger))] static class NoAchievement { static bool Prefix() => false; }
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Initialized), MethodType.Getter)] static class Offline { static bool Prefix(ref bool __result) { __result = false; return false; } }

        // Tests exercise native Apply/Cancel/Reset against private in-memory input
        // preferences. They never overwrite the user's actual registry settings.
        [HarmonyPatch]
        static class IsolatedBindings
        {
            static IEnumerable<MethodBase> TargetMethods() => AccessTools.GetDeclaredMethods(typeof(KeyBindingsUI));
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    if (instruction.operand is MethodInfo method && method.DeclaringType == typeof(PlayerPrefs))
                        instruction.operand = AccessTools.Method(typeof(MemoryPrefs), method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray());
                    yield return instruction;
                }
            }
        }
        static class MemoryPrefs
        {
            static readonly Dictionary<string, string> data = new Dictionary<string, string>();
            public static bool HasKey(string key) => data.ContainsKey(key);
            public static string GetString(string key) => data.TryGetValue(key, out var value) ? value : "";
            public static void SetString(string key, string value) => data[key] = value;
            public static void DeleteKey(string key) => data.Remove(key);
            public static void Save() { }
        }
    }
}
