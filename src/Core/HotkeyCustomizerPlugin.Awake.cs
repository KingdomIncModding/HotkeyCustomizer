using System;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
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

namespace KingdomHotkeyCustomizer
{
    public sealed partial class HotkeyCustomizerPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Instance = this;
            foundation = FoundationApi.ForMod(Id);
            research = foundation.Register(new TechNodeDefinition(ResearchId, "Hotkey Customizer", Id, "Hotkey Customizer",
                description: "Unlock ten custom building shortcuts and a vertical building hotbar. Configure them in Options > Mods > Hotkey Customizer. Free research.",
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
            foreach (var type in new[] { typeof(ApplySettingsPatch), typeof(CancelSettingsPatch), typeof(ResetSettingsPatch),
                typeof(NativeRebindPatch), typeof(VanillaBindingLoadPatch), typeof(VanillaBindingSnapshotPatch),
                typeof(VanillaBindingLabelPatch), typeof(CreateHotbarPatch) })
                harmony.CreateClassProcessor(type).Patch();
            Logger.LogInfo("Hotkey Customizer ready.");
        }
    }
}
