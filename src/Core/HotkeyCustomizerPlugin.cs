using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    [BepInPlugin(Id, "Hotkey Customizer", Version)]
    [BepInDependency("urakka.kingdominc.foundation", "0.14.0")]
    public sealed class HotkeyCustomizerPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Instance = this;
            Api.HotkeyCustomizerApi.Initialize();
            foundation = FoundationApi.ForMod(Id);
            research = foundation.Register(new TechNodeDefinition(ResearchId, "Hotkey Customizer", Id, "Hotkey Customizer", description: "Unlock ten custom building shortcuts and a vertical building hotbar. Configure them in Options > Mods > Hotkey Customizer. Free research.", research: TechNodeOptions.Create(nodeKind: "Utility", fixedCost: new GameMoney(), iconName: "Icons_17")));
            InputSystem.RegisterInteraction<ShortcutModifiers>(VanillaBindings.Interaction);
            ShowHotbar = Config.Bind("Interface", "ShowHotbar", true, "Show the vertical building hotbar on the right edge. Hover to reveal it. Keyboard shortcuts work when hidden.");
            for (int slot = 0; slot < 10; slot++)
            {
                Buildings[slot] = Config.Bind("Buildings", "Slot" + slot, "", "Building type for entry " + slot + ". Choose in Options > Mod Settings; empty means Unassigned.");
                Bindings[slot] = Config.Bind("Hotkeys", "Slot" + slot, "", "User-assigned shortcut for entry " + slot + ". Empty by default. Use the in-game key button to assign it.");
                int index = slot;
                Bindings[slot].SettingChanged += (s, e) => Shortcuts[index] = BuildingShortcut.Parse(Bindings[index].Value);
                Shortcuts[slot] = BuildingShortcut.Parse(Bindings[slot].Value);
            }

            gameObject.AddComponent<BuildingSettings>();
            gameObject.AddComponent<ModHotkeyCapture>();
            RegisterHooks();
            Logger.LogInfo("Hotkey Customizer ready.");
        }

        internal static bool CanUseBuildings()
        {
            return Unlocked && CanUseGameplayHotkeys();
        }

        internal static bool CanUseGameplayHotkeys()
        {
            if (!FoundationApi.UiAccess.Root || !FoundationApi.World.IsInKingdom || !FoundationApi.View.GameInputEnabled || Time.timeScale == 0 || !FoundationApi.UiAccess.IsWorkspaceVisible || FoundationApi.UiAccess.IsMenuVisible || FoundationApi.UiAccess.IsAlertVisible)
                return false;
            if (ModHotkeyCapture.Instance?.IsListening == true || BuildingSettings.Instance?.Listening >= 0 || BuildingSettings.Instance?.ListeningAction != null)
                return false;
            var selected = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            if (selected && (selected.GetComponentInParent<TMP_InputField>() || selected.GetComponentInParent<TMP_Dropdown>()))
                return false;
            if (FoundationApi.UiAccess.HasPopup || FoundationApi.UiAccess.IsMenuOpen(GameMenu.Research) || FoundationApi.UiAccess.IsMenuOpen(GameMenu.Clone) || FoundationApi.UiAccess.IsMenuOpen(GameMenu.AreaMove))
                return false;
            return !global::UnityEngine.Object.FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).Any(field => field.isFocused);
        }

        public const string Id = "urakka.kingdominc.hotkeycustomizer";
        public const string Version = "1.5.1";
        public const string ResearchId = Id + ".unlock";
        internal static HotkeyCustomizerPlugin Instance;
        private FoundationModContext foundation;
        private ITechNode research;
        private readonly System.Collections.Generic.List<IDisposable> hooks = new System.Collections.Generic.List<IDisposable>();
        internal readonly ConfigEntry<string>[] Buildings = new ConfigEntry<string>[10];
        internal readonly ConfigEntry<string>[] Bindings = new ConfigEntry<string>[10];
        internal readonly BuildingShortcut[] Shortcuts = new BuildingShortcut[10];
        internal ConfigEntry<bool> ShowHotbar;
        internal static bool Unlocked => Instance != null && Instance.research?.IsRegistered == true && FoundationApi.World.IsInKingdom && FoundationApi.Progression.HasTech(ResearchId);

        private void RegisterHooks()
        {
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BindingsApply, before: call => ApplySettingsPatch.Prefix(), after: call => ApplySettingsPatch.Postfix()));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BindingsCancel, before: call => CancelSettingsPatch.Prefix()));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BindingsReset, after: call => ResetSettingsPatch.Postfix()));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BindingRowClick, before: call =>
            {
                if (!NativeRebindPatch.Prefix(call.Argument<PointerEventData>(0), FoundationApi.Bindings.Action(call.View), FoundationApi.Bindings.Text(call.View)))
                    call.Cancel();
            }));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BindingsLoad, after: call => VanillaBindingLoadPatch.Postfix()));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BindingsOpened, after: call =>
            {
                var snapshot = FoundationApi.Bindings.Snapshot(call.View);
                VanillaBindingSnapshotPatch.Postfix(ref snapshot);
                FoundationApi.Bindings.SetSnapshot(call.View, snapshot);
            }));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BindingRowText, after: call => VanillaBindingLabelPatch.Postfix(call.View, FoundationApi.Bindings.Action(call.View), FoundationApi.Bindings.Text(call.View))));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.UiStarted, after: call => CreateHotbarPatch.Postfix(call.View)));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.MenuUpdate, before: call =>
            {
                if (!CaptureMenuEscapePatch.Prefix())
                    call.Cancel();
            }));
            hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.Escape, before: call =>
            {
                if (!CaptureGameEscapePatch.Prefix())
                    call.Cancel();
            }));
        }

        private void DisposeHooks()
        {
            foreach (var hook in hooks)
                hook.Dispose();
            hooks.Clear();
        }

        private void OnDestroy()
        {
            Api.HotkeyCustomizerApi.Shutdown();
            DisposeHooks();
            foundation?.Dispose();
            if (Instance == this)
                Instance = null;
        }

        internal void ReportHotkeyError(string message) => Logger.LogError(message);
        internal bool SelectBuilding(int slot)
        {
            if (slot < 0 || slot >= Buildings.Length || !CanUseBuildings())
                return false;
            string id = Buildings[slot].Value;
            if (string.IsNullOrEmpty(id))
                return false;
            var player = FoundationApi.Kingdom;
            var element = FoundationApi.WorkspaceAccess.Catalog.FirstOrDefault(e => e.TypeId == id && e.RequiredResearch != null);
            if (element == null || !FoundationApi.WorkspaceAccess.CanPurchase(element))
                return false;
            if (!player.CanAfford(FoundationApi.WorkspaceAccess.PurchaseCost(element)))
            {
                FoundationApi.UiAccess.PlaySound(GameUiSound.CraftStep);
                FoundationApi.UI.ShowInsufficientFundsWarning();
                return false;
            }

            // This is the game's preview; a shortcut never purchases a machine itself.
            FoundationApi.WorkspaceAccess.StopBlueprintMode();
            FoundationApi.WorkspaceAccess.ShowPurchaseBlueprint(element, false);
            return true;
        }

        internal IModConfigSection SettingsSection() => foundation.Settings("Hotkey Customizer", "Configure building shortcuts (slots 0-9) and the building hotbar.");
        private void Update()
        {
            Api.HotkeyCustomizerApi.Update();
            var keyboard = Keyboard.current;
            if (keyboard == null || ModHotkeyCapture.Instance?.IsListening == true || BuildingSettings.Instance?.Listening >= 0 || BuildingSettings.Instance?.ListeningAction != null)
                return;
            for (int slot = 0; slot < 10; slot++)
                if (Shortcuts[slot].Pressed(keyboard))
                {
                    SelectBuilding(slot);
                    break;
                }
        }
    }
}
