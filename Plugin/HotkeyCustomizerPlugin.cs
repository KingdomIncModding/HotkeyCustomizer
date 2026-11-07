using System;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using Assets.Source.Flow;
using Assets.Source.Player;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace KingdomHotkeyCustomizer
{
    [BepInPlugin(Id, "Hotkey Customizer", Version)]
    [BepInDependency("urakka.kingdominc.foundation", "0.5.0")]
    public sealed partial class HotkeyCustomizerPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.hotkeycustomizer";
        public const string Version = "1.4.0";
        public const string ResearchId = Id + ".unlock";
        ITechNode research;
        internal static bool Unlocked => Instance != null && Instance.research?.IsRegistered == true
            && GamePlayer.Current != null && GamePlayer.Current.HasTech(ResearchId);
        internal static HotkeyCustomizerPlugin Instance;
        internal static bool Diagnostic => Environment.GetCommandLineArgs().Contains("-hotkeys-smoke");
        internal readonly ConfigEntry<string>[] Buildings = new ConfigEntry<string>[10];
        internal readonly ConfigEntry<string>[] Bindings = new ConfigEntry<string>[10];
        internal readonly BuildingShortcut[] Shortcuts = new BuildingShortcut[10];
        internal ConfigEntry<bool> ShowHotbar;
        Harmony harmony;

        void Awake()
        {
            Instance = this;
            research = FoundationApi.CreateTechNode(new TechNodeDefinition(ResearchId, "Hotkey Customizer", Id, "Hotkey Customizer",
                description: "Unlock ten custom building shortcuts and a vertical building hotbar. Configure them in Options > Mod Settings > Hotkey Customizer. Free research.",
                research: new TechNodeOptions(nodeType: TechNodeType.Utility, fixedCost: new CurrencyCollection(), iconName: "Icons_17")));
            InputSystem.RegisterInteraction<ShortcutModifiers>(VanillaBindings.Interaction);
            ShowHotbar = Config.Bind("Interface", "ShowHotbar", true,
                "Show the vertical building hotbar on the right edge. Hover to reveal it. Keyboard shortcuts work when hidden.");
            for (int slot = 0; slot < 10; slot++)
            {
                Buildings[slot] = Config.Bind("Buildings", "Slot" + slot, "",
                    "Building type for entry " + slot + ". Choose in Options > Mod Settings; empty means Unassigned.");
                Bindings[slot] = Config.Bind("Hotkeys", "Slot" + slot, "",
                    "User-assigned shortcut for entry " + slot + ". Empty by default. Use the in-game key button to assign it.");
                int index = slot;
                Bindings[slot].SettingChanged += (s, e) => Shortcuts[index] = BuildingShortcut.Parse(Bindings[index].Value);
                Shortcuts[slot] = BuildingShortcut.Parse(Bindings[slot].Value);
            }
            gameObject.AddComponent<BuildingSettings>();
            harmony = new Harmony(Id);
            foreach (var type in new[] { typeof(ApplySettings), typeof(CancelSettings), typeof(ResetSettings), typeof(NativeRebind),
                typeof(VanillaBindings.Load), typeof(VanillaBindings.Snapshot), typeof(VanillaBindings.Label), typeof(CreateHotbar) })
                harmony.CreateClassProcessor(type).Patch();
            if (Diagnostic)
            {
                InstallDiagnostics();
                Application.runInBackground = true;
                StartCoroutine(RuntimeCheck());
            }
            Logger.LogInfo("Hotkey Customizer ready: vanilla key combinations and ten building shortcuts. Existing assignments are preserved.");
        }

        void OnDestroy() { harmony?.UnpatchSelf(); research?.Dispose(); if (Instance == this) Instance = null; }

        internal static bool CanUseBuildings()
        {
            if (!Unlocked || !GameUI.Instance || !GameInput.Enabled || Time.timeScale == 0
                || !WorldFlowManager.Visible || UIAlertWindow.Active) return false;
            var selected = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
            if (selected && (selected.GetComponentInParent<TMP_InputField>() || selected.GetComponentInParent<TMP_Dropdown>())) return false;
            if (GameUI.Instance.IsActive(GameUIType.Popup) || GameUI.Instance.IsActive(GameUIType.Tech)
                || GameUI.Instance.IsActive(GameUIType.Clone) || GameUI.Instance.IsActive(GameUIType.AreaMove)) return false;
            return !UnityEngine.Object.FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).Any(f => f.isFocused);
        }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            for (int slot = 0; slot < 10; slot++)
                if (Shortcuts[slot].Pressed(keyboard)) { SelectBuilding(slot); break; }
        }

        internal bool SelectBuilding(int slot)
        {
            if (slot < 0 || slot > 9 || !CanUseBuildings()) return false;
            string id = Buildings[slot].Value;
            if (string.IsNullOrEmpty(id)) return false;
            var element = FlowElement.GetAllTypes().FirstOrDefault(e => e.ElementType == id && e.RequiredTech != null);
            // Match the buy menu. Never create a building directly or bypass research/cost.
            if (element == null || !element.IsPurchaseable(GamePlayer.Current)) return false;
            if (!GamePlayer.Current.CanAfford(element.GetPurchaseCost()))
            {
                UISounds.CraftStep(); GameUI.Instance.InsufficientFundsWarning(); return false;
            }
            WorldFlowManager.Instance.StopBlueprintMode();
            WorldFlowManager.Instance.ShowPurchaseBlueprint(element, false);
            return true;
        }

        [HarmonyPatch(typeof(GameUI), "Start")]
        static class CreateHotbar
        {
            static void Postfix(GameUI __instance) => __instance.gameObject.AddComponent<BuildingHotbar>().Build(__instance);
        }

        [HarmonyPatch(typeof(KeyBindingsUI), nameof(KeyBindingsUI.ApplyBindings))]
        static class ApplySettings
        {
            static void Prefix(KeyBindingsUI __instance)
            {
                VanillaBindings.StripInteractions();
                BuildingSettings.Instance?.Commit();
            }
            static void Postfix() => VanillaBindings.ApplySaved();
        }

        [HarmonyPatch(typeof(KeyBindingsUI), nameof(KeyBindingsUI.CancelBindings))]
        static class CancelSettings
        {
            static void Prefix() => BuildingSettings.Instance?.Cancel();
        }

        [HarmonyPatch(typeof(KeyBindingsUI), nameof(KeyBindingsUI.ResetBindings))]
        static class ResetSettings
        {
            static void Postfix() => BuildingSettings.Instance?.PendingVanilla.Clear();
        }

        [HarmonyPatch(typeof(KeyBindingRow), nameof(KeyBindingRow.OnPointerClick))]
        static class NativeRebind
        {
            static bool Prefix(KeyBindingRow __instance, UnityEngine.EventSystems.PointerEventData eventData, InputAction ____bindingRef, TMP_Text ____binding)
            {
                if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left && BuildingSettings.Instance != null)
                    BuildingSettings.Instance.ListenNative(____bindingRef, ____binding);
                return false;
            }
        }

        internal static BuildingSettings SettingsFor(KeyBindingsUI ui) => BuildingSettings.Instance;
    }
}
