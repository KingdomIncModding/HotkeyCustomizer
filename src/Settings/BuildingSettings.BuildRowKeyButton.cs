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
        private void BuildRowKeyButton(IModConfigContext context, Transform parent, int slot)
        {
            context.Templates.CreateButtonRow(parent, "", BuildingShortcut.Parse(PendingKeys[slot]).Display,
                () => Listen(slot), out var button, out var keyLabel);
            button.name = "KeyButton_" + slot;
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(.70f, .04f);
            rect.anchorMax = new Vector2(.98f, .96f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            KeyButtons.Add(button);
            button.gameObject.AddComponent<NativeKeyHover>().Target = button.GetComponent<Image>();
            var nativeRow = Resources.FindObjectsOfTypeAll<KeyBindingRow>().FirstOrDefault();
            if (nativeRow != null && keyLabel != null)
            {
                var nativeText = (TMP_Text)AccessTools.Field(typeof(KeyBindingRow), "_binding")?.GetValue(nativeRow);
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
    }
}
