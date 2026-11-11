using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    internal partial struct BuildingShortcut
    {
        internal static BuildingShortcut Parse(string text)
        {
            var result = new BuildingShortcut();
            if (string.IsNullOrWhiteSpace(text)) return result;
            var parts = text.Split('+');
            if (!Enum.TryParse(parts[parts.Length - 1], out result.Key) || !Enum.IsDefined(typeof(Key), result.Key)
                || result.Key == Key.None || Modifier(result.Key)) return default;
            for (int i = 0; i < parts.Length - 1; i++)
                switch (parts[i])
                {
                    case "Ctrl": result.Ctrl = true; break;
                    case "Alt": result.Alt = true; break;
                    case "Shift": result.Shift = true; break;
                    case "Meta": result.Meta = true; break;
                    default: return default;
                }
            return result;
        }
    }
}
