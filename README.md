# Hotkey Customizer

**Current release: 1.5.1 — for Kingdom Inc game version 1.0.14.**

[GitHub releases](https://github.com/KingdomIncModding/HotkeyCustomizer/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/16) · Version 1.5.1

Vanilla key combinations, ten user-configurable building shortcuts, and a collapsible building hotbar for Kingdom Inc.

Add modifier combinations (Ctrl, Alt, Shift, Meta) to vanilla shortcuts, assign custom keys for your favorite buildings, and quickly place structures from a responsive edge hotbar.

## Features

- **Multi-Key Combinations:** Rebind vanilla shortcuts to combinations such as Ctrl + Shift + 1.
- **Ten Custom Building Shortcuts:** Configure shortcuts for up to 10 specific buildings (Building 0–9) under Options > Mod Settings > Hotkey Customizer.
- **Modifier Key Support:** Assign custom key chords with Ctrl, Alt, Shift, or Meta modifiers.
- **Collapsible Edge Hotbar:** Access a responsive vertical hotbar at the right screen edge. Hover to reveal, move away to retract, or click icons for direct placement.
- **Foundation Settings Integration:** Configure building hotkeys and hotbar toggle directly in-game via Options > Mod Settings > Hotkey Customizer.
- **Per-Kingdom Research Unlock:** Research Hotkey Customizer for free under Tech Tree > Mods to unlock building shortcuts and the hotbar in each kingdom.

- **Per-Section Hotkeys:** Registered hotkey groups appear in their own settings sections. Keys are saved separately, with Apply, Cancel and Reset Hotkeys controls.

## Usage and Controls

1. Research **Hotkey Customizer** in **Tech Tree > Mods** for free.
2. Open **Options > Key Bindings** for vanilla controls, or **Options > Mod Settings > Hotkey Customizer** for building shortcuts.
3. **Vanilla Shortcuts:** Click any vanilla key field and press your desired key or combination with modifiers.
4. **Building Shortcuts:** Under **Custom Hotkeys (Building 0–9)**, select a building from the dropdown on the left and assign a key combination on the right.
5. Click **Apply** to save changes. **Cancel** discards pending edits. Switching sections keeps edits pending.

Registered hotkey groups appear under their own names in **Options > Mod Settings**. Click a key field, hold modifiers, then press the final key. Escape cancels capture; Backspace or Delete clears the key. **Reset Hotkeys** restores only that section's defaults when applied. Groups appear when an integration requests them.

## Requirements

- Kingdom Inc (Windows x64 Mono)
- [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6) (5.4.23.6 or newer)
- [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.14.0 or newer)

## Installation

1. Install BepInEx 5 for Kingdom Inc.
2. Install Foundation (0.14.0 or newer).
3. Extract the release ZIP into your Kingdom Inc game directory.
4. Verify the file exists at `BepInEx/plugins/HotkeyCustomizer/HotkeyCustomizer.dll`.
5. Launch the game and unlock Hotkey Customizer in Tech Tree > Mods!

## Compatibility

| Class.Method | Hook Type | Impact |
|---|---|---|
| KeyBindingsUI.ApplyBindings | Foundation hooks (prefix and postfix) | Saves shortcut edits and restores modifier combinations. |
| KeyBindingsUI.CancelBindings | Foundation hook (prefix) | Discards pending shortcut changes. |
| GameUI.Start | Foundation hook (postfix) | Adds the building hotbar to the game HUD. |
| KeyBindingRow.OnPointerClick | Foundation hook (prefix) | Captures a key with optional modifiers. |
| KeyBindingsUI.ResetBindings | Foundation hook (postfix) | Clears pending vanilla modifier overrides. |
| KeyBindingRow._resetBindingText | Foundation hook (postfix) | Displays the assigned key combination. |
| KeyBindingsUI.Apply | Foundation hook (postfix) | Restores saved vanilla modifier combinations. |
| KeyBindingsUI.OnEnable | Foundation hook (postfix) | Keeps a snapshot for Cancel. |
| MainMenuUI.Update | Foundation hook (prefix) | Keeps Escape within an active key capture. |
| GameUI.ProcessEscape | Foundation hook (prefix) | Cancels key capture without closing its menu. |

## Removal

Close the game and delete `BepInEx/plugins/HotkeyCustomizer/`.

## Credits and Source

Created by **Urakka** (rockyamyx). [Source code on GitHub](https://github.com/KingdomIncModding/HotkeyCustomizer). Game artwork belongs to its respective owners.
