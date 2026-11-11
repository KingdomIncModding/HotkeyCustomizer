using System;
using System.Collections.Generic;
using System.Linq;
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

namespace KingdomHotkeyCustomizer
{
    public sealed partial class BuildingSettings : MonoBehaviour
    {
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
    }
}
