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
    [BepInPlugin(Id, "Hotkey Customizer", Version)]
    [BepInDependency("urakka.kingdominc.foundation", "0.11.0")]
    public sealed partial class HotkeyCustomizerPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.hotkeycustomizer";
        public const string Version = "1.4.0";
        public const string ResearchId = Id + ".unlock";
        internal static HotkeyCustomizerPlugin Instance;
        private FoundationModContext foundation;
        private ITechNode research;
        private Harmony harmony;
        internal readonly ConfigEntry<string>[] Buildings = new ConfigEntry<string>[10];
        internal readonly ConfigEntry<string>[] Bindings = new ConfigEntry<string>[10];
        internal readonly BuildingShortcut[] Shortcuts = new BuildingShortcut[10];
        internal ConfigEntry<bool> ShowHotbar;
        internal static bool Unlocked => Instance != null && Instance.research?.IsRegistered == true
            && FoundationApi.World.Player != null && FoundationApi.World.Player.HasTech(ResearchId);
    }
}
