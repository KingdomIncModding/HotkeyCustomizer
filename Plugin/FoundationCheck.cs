using System;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.MainMenu;
using Assets.Behaviour.UI.TechTree;
using Assets.Source.Player;
using BepInEx.Bootstrap;
using HarmonyLib;
using KingdomFoundation;
using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class HotkeyCustomizerPlugin
    {
        void VerifyResearchLocked(BuildingHotbar hotbar, Keyboard keyboard)
        {
            Check(Chainloader.PluginInfos.TryGetValue("urakka.kingdominc.foundation", out var info)
                && info.Metadata.Version >= new Version(0, 5, 0), "Foundation dependency loads before Hotkey Customizer");
            Check(FoundationApi.TechNodes.TryGet(ResearchId, out var registered) && ReferenceEquals(registered, research)
                && registered is INativeTechNode, "Hotkey Customizer registers a native Foundation technology");
            Check(((IModTechNode)registered).Mod.Id == Id && ((IModTechNode)registered).Mod.Name == "Hotkey Customizer",
                "Research belongs to the Hotkey Customizer sidebar category");
            Check(!Unlocked && !CanUseBuildings(), "New kingdom starts with custom building shortcuts locked");
            Keys(keyboard); Keys(keyboard, Key.LeftCtrl, Key.F6); Update();
            Check(!WorldFlowManager.Instance.BlueprintActive, "Assigned building hotkey cannot bypass unresearched technology");
            Keys(keyboard);
            hotbar.Items[0].OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Check(!WorldFlowManager.Instance.BlueprintActive, "Hotbar click cannot bypass unresearched technology");
            ShowHotbar.Value = true; hotbar.Refresh();
            Check(!hotbar.Strip.gameObject.activeSelf, "Hotbar remains hidden before its technology is researched");
            ShowHotbar.Value = false;
            var pause = Resources.FindObjectsOfTypeAll<InGameMenuUI>().First();
            pause.gameObject.SetActive(true); pause.ShowOptions();
            var options = Resources.FindObjectsOfTypeAll<OptionsUI>().First(o => o.gameObject.activeInHierarchy);
            var configMenu = options.GetComponent<KingdomFoundation.FoundationConfigMenu>() ?? KingdomFoundation.FoundationConfigMenu.Attach(options);
            configMenu.ShowModsTab(Id);
            var settings = SettingsFor(null);
            Check(settings.ResearchNotice && settings.Dropdowns.Count == 10 && settings.PendingKeys[0] == "Ctrl+F6",
                "Locked kingdom can edit saved assignments and sees where to research them");
            options.CancelOptions(); pause.DoResume();
        }

        void OpenResearch()
        {
            GameUI.Instance.ToggleTechMenu(true);
            var tree = TechTreeMenu.Instance;
            Check(!tree.Nodes.Any(n => n.Tech.Identifier == ResearchId), "Hotkey Customizer does not appear in Base Game research");
            var tabs = tree.GetComponent<TechTreeTabs>();
            Check(tabs, "Foundation supplies native research tabs");
            AccessTools.Method(typeof(TechTreeTabs), "SelectMod").Invoke(tabs, new object[] { Id, false });
        }

        void VerifyResearchPurchase()
        {
            var node = TechTreeMenu.Instance.Nodes.Single(n => n.Tech.Identifier == ResearchId);
            Check(node.Tech == ((INativeTechNode)research).NativeNode && node.Tech.Name == "Hotkey Customizer"
                && node.Tech.IsAvailable && !node.Tech.IsPurchased, "Mods category displays the available native Hotkey Customizer technology");
            var player = GamePlayer.Current;
            var funds = player.GetCurrency(Currency.Gold);
            node.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right });
            Check(!Unlocked, "Right click does not unlock technology");
            node.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Check(Unlocked && node.Tech.IsPurchased && player.GetCurrency(Currency.Gold) == funds,
                "Native left-click research unlocks Hotkey Customizer for free");
            node.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Check(player.GetCurrency(Currency.Gold) == funds, "Purchased technology cannot charge twice");
            var json = player.ToJson();
            Check(json["Tech"].AsJsonArray.Any(v => v.AsString == ResearchId), "Unlock is stored in the native saved technology list");
            try
            {
                GamePlayer.Current = GamePlayer.FromJson(json);
                Check(Unlocked, "Research unlock survives a native save round trip");
                GamePlayer.CreateNewPlayer();
                Check(!Unlocked, "A separate new kingdom does not inherit the research unlock");
            }
            finally { GamePlayer.Current = player; }
            Check(Buildings[0].Value == "Mine" && Bindings[0].Value == "Ctrl+F6", "Research changes do not overwrite saved hotkey assignments");
        }
    }
}
