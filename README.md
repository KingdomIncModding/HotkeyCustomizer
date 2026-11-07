# Hotkey Customizer

Customize vanilla shortcuts and set up ten building shortcuts with an optional vertical hotbar. All new building and key entries start **Unassigned**. Existing workspace shortcuts stay as assigned.

[Downloads](https://github.com/urakkaamyx/KingdomInc_HotkeyCustomizer/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/16)

## Research

Open **Tech Tree > Mods > Hotkey Customizer** and research **Hotkey Customizer** for free to unlock building shortcuts and the hotbar in that kingdom. You can configure assignments before researching. Vanilla key combinations remain available.

## Key bindings

1. Open **Options > Key Bindings** or **Options > Mods > Hotkey Customizer** from the main menu or pause menu.
2. Click a vanilla key field to change that shortcut. Under **Custom Hotkeys**, choose a building on the left and click its key field on the right.
3. Hold any desired Ctrl, Alt, Shift or Windows/Meta modifiers, then press the final key. For example, **Ctrl + Shift + 1**. Left and right modifiers work interchangeably.
4. Click **Apply & Close** to save. **Cancel** discards pending edits.

During capture, **Backspace** or **Delete** clears the key; **Escape** cancels capture. **Reset to default** resets vanilla bindings, clears the ten custom building entries and restores Show Hotbar to enabled when applied.

Entry numbers 0–9 are labels, not assigned keys. Avoid overlaps with existing shortcuts. If custom entries share a shortcut, the lowest-numbered entry takes priority. Reassigned vanilla combinations require exactly the chosen modifiers.

## Hotbar

Enable **Show Hotbar** under **Custom Hotkeys**. Hover the bar's exposed edge on the right to reveal it; move away to retract it. Black tiles show building icons and small key labels. Building names appear in hover tooltips.

Click an icon or press its assigned shortcut to open normal building placement. Buildings still require their own research and purchase cost. Locked icons are dimmed. Hiding the hotbar leaves keyboard shortcuts active. Placement shortcuts pause during menus, text entry, alerts and conflicting tools.

## Requirements and installation

Requires the Windows Mono version of Kingdom Inc., BepInEx 5, and Foundation 0.5.0 or newer. Install these dependencies separately.

Close the game and extract the ZIP into the Kingdom Inc. installation folder. The plugin belongs at:

`BepInEx/plugins/HotkeyCustomizer/HotkeyCustomizer.dll`

For updates, replace that DLL with the game closed. Existing assignments and Show Hotbar preferences are kept in `BepInEx/config/urakka.kingdominc.hotkeycustomizer.cfg`. Research is saved per kingdom.

To uninstall, close the game and remove the HotkeyCustomizer plugin folder. You can also remove its configuration file to discard assignments. Removing the plugin leaves reassigned vanilla combinations as ordinary bindings using their final key. Use the game's Reset to default option to restore defaults.

## Changelog

- **1.4.0**: Added Foundation ModConfig menu integration under Options > Mods tab.
- **1.3.0**: Foundation tech research requirement.
- **1.2.1**: Building hotbar integration.
- **1.1.0**: Initial release.

## Credits

Created by Urakka. Kingdom Inc. and its artwork belong to their respective owners. The package contains no game assemblies or extracted artwork.

Generative AI was used for code, documentation and promotional thumbnail artwork.
