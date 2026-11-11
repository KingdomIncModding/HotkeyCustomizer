# Hotkey Customizer

[GitHub releases](https://github.com/urakkaamyx/KingdomInc_HotkeyCustomizer/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/16) · Version 1.4.0

Vanilla key combinations, ten user-configurable building shortcuts, and a collapsible building hotbar for Kingdom Inc.

Add modifier combinations (Ctrl, Alt, Shift, Meta) to vanilla shortcuts, assign custom keys for your favorite buildings, and quickly place structures from a responsive edge hotbar.

## Features

- **Multi-Key Combinations:** Rebind vanilla shortcuts to combinations such as Ctrl + Shift + 1.
- **Ten Custom Building Shortcuts:** Configure shortcuts for up to 10 specific buildings (Building 0–9) under Options > Key Bindings > Custom Hotkeys.
- **Modifier Key Support:** Assign custom key chords with Ctrl, Alt, Shift, or Meta modifiers.
- **Collapsible Edge Hotbar:** Access a responsive vertical hotbar at the right screen edge. Hover to reveal, move away to retract, or click icons for direct placement.
- **Foundation Settings Integration:** Configure building hotkeys and hotbar toggle directly in-game via Options > Key Bindings or the Foundation Mods menu.
- **Per-Kingdom Research Unlock:** Research Hotkey Customizer for free under Tech Tree > Mods to unlock building shortcuts and the hotbar in each kingdom.

## Usage and Controls

1. Research **Hotkey Customizer** in **Tech Tree > Mods** for free.
2. Open **Options > Key Bindings**.
3. **Vanilla Shortcuts:** Click any vanilla key field and press your desired key or combination with modifiers.
4. **Building Shortcuts:** Scroll to **Custom Hotkeys (Building 0–9)**, select a building from the dropdown on the left, and assign a key combination on the right.
5. Click **Apply** to save changes.

## Requirements

- Kingdom Inc (Windows x64 Mono)
- [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6) (5.4.23.6 or newer)
- [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.11.0 or newer)

## Installation

1. Install BepInEx 5 for Kingdom Inc.
2. Install Foundation (0.11.0 or newer).
3. Extract the release ZIP into your Kingdom Inc game directory.
4. Verify the file exists at `BepInEx/plugins/HotkeyCustomizer/HotkeyCustomizer.dll`.
5. Launch the game and unlock Hotkey Customizer in Tech Tree > Mods!

## Compatibility

| Class.Method | Hook Type | Impact |
|---|---|---|
| KeyBindingsUI.ApplyBindings | Harmony Postfix | Applies and saves custom modifier and building hotkey assignments. |
| KeyBindingsUI.CancelBindings | Harmony Postfix | Discards unsaved custom hotkey modifications when cancelling the menu. |
| GameUI.Start | Harmony Postfix | Creates and attaches the collapsible building hotbar to the game HUD. |
| KeyBindingRow.OnPointerClick | Harmony Prefix | Intercepts native rebind clicks to capture modifier-enhanced multi-key chords. |
| KeyBindingsUI.ResetBindings | Harmony Postfix | Resets pending custom hotkey bindings to their default values. |
| KeyBindingRow._resetBindingText | Harmony Postfix | Formats key binding text labels to display modifier combinations cleanly. |
| KeyBindingsUI.Apply | Harmony Postfix | Loads and activates persistent vanilla modifier overrides on game start. |
| KeyBindingsUI.OnEnable | Harmony Postfix | Takes a snapshot of active bindings when the keybindings UI opens. |

## Removal

Close the game and delete `BepInEx/plugins/HotkeyCustomizer/`.

## Credits and Source

Created by **Urakka** (rockyamyx). [Source code on GitHub](https://github.com/urakkaamyx/KingdomInc_HotkeyCustomizer). Game artwork belongs to its respective owners.
