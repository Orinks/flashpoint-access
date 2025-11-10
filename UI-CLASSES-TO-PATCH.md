# UI Classes to Patch for Accessibility

Based on the IL2CPP dump analysis, here are the key UI classes that should be patched for screen reader accessibility:

## Core UI Base Classes
- `UIScreenBase` (line 34164) - Base class for all screens
- `SafehouseUIScreenBase` - Base for safehouse/hub screens
- `STEWidgetBase` (line 39044) - Base class for all UI widgets
- `STEWidgetInputBase` (line 39138) - Base for interactive widgets
- `STESelectableWidgetBase` - Base for selectable elements

## Screen Classes (Main Game Screens)
- `Screen_MissionPlanning` (line 5535) - Mission planning screen
- `Screen_MissionPreamble` (line 5769) - Mission briefing
- `Screen_Cyberdeck` (line 6091) - Cyberdeck management
- `Screen_ImplantList` (line 6163) - Implant inventory
- `Screen_Loadout` (line 6175) - Character loadout screen
- `Screen_LoadoutEdit` (line 6191) - Loadout editing
- `Screen_Training` (line 6291) - Training interface
- `Screen_TrainingMultiClass` (line 6427) - Multi-class training
- `Screen_TrainingPreview` (line 6490) - Training preview
- `Screen_ContactsList` (line 9061) - Contacts management
- `Screen_CyberClinic` (line 9234) - Cyber clinic screen
- `Screen_ColdStorage` (line 9015) - Storage management
- `Screen_Roster` - Team roster screen

## Button Classes (Interactive Elements)
- `STEButton` (line 56611) - **PRIMARY TARGET** - Main button class
- `STEButtonGlyph` (line 55743) - Button with glyph/icon
- `STEButtonImageGlyph` (line 55876) - Image-based button
- `STEButtonAppearanceTab` (line 56746) - Appearance tab button
- `STEButtonColor` (line 56776) - Color selection button
- `STEButtonSafehouseCharacter` (line 13916) - Character selection button
- `STEButtonSafehouseRoom` (line 13978) - Room navigation button
- `STEDialogAnswerButton` (line 45477) - Dialog choice button
- `STEButtonAction` (line 55633) - Action button
- `STEAddSubtractButtons` (line 56533) - +/- buttons

## Text Classes (Information Display)
- `STETextBlock` (line 41500) - **PRIMARY TARGET** - Main text display
- `STEAdvancedTextBlock` (line 41249) - Advanced text formatting
- `STESelectableTextBlock` (line 41356) - Selectable text
- `STEMoneyTextBlock` (line 41330) - Currency display
- `STETimeTextBlock` (line 41650) - Time display
- `STEWarningTextBlock` (line 41711) - Warning messages
- `STEDialogCharacterName` (line 45628) - Dialog character names
- `STEDialogMessage` (line 45738) - Dialog text
- `STETextInput` (line 43927) - Text input field

## List/Panel Classes (Navigation)
- `STEPanelVerticalScrolling` - Scrollable panels
- `View_ContactListPanel_MessagesHelper` (line 5397) - Message lists
- `View_RosterPanel_Info` (line 8110) - Roster info panel
- `View_Inventory` (line 7178) - Inventory view
- `View_EditLoadoutList` (line 7037) - Loadout list
- `STEAdvancedTextList` (line 45896) - Text list view

## Special UI Elements
- `STETextSlider` (line 41589) - Text-based slider
- `STEMatrixButtonBlock` (line 51747) - Button grid/matrix
- `STETextItem` (line 54882) - Text item in lists
- `STEEffectTextItem` (line 37973) - Effect descriptions
- `TutorialHelpButton` (line 3386) - Tutorial help

## Priority Patching Strategy

### Phase 1 - Critical Base Classes (Highest Impact)
1. **STEButton** - All button interactions
2. **STETextBlock** - All text display
3. **UIScreenBase** - Screen transitions/context

### Phase 2 - Navigation & Selection
4. **STEWidgetInputBase** - Input handling
5. **STESelectableWidgetBase** - Selection feedback
6. **STEButtonGlyph** - Icon buttons

### Phase 3 - Specific Screens
7. **Screen_MissionPlanning** - Core gameplay
8. **Screen_Roster** - Character management
9. **Screen_Loadout** - Equipment management

### Phase 4 - Lists & Panels
10. Scrolling panels and list views
11. Inventory and equipment views

## Recommended Harmony Patches

```csharp
// Patch button clicks to announce via SRAL
[HarmonyPatch(typeof(STEButton), "OnPointerClick")]
[HarmonyPostfix]
static void AnnounceButtonClick(STEButton __instance) { }

// Patch text updates to speak changes
[HarmonyPatch(typeof(STETextBlock), "SetText")]
[HarmonyPostfix]
static void AnnounceTextChange(STETextBlock __instance, string text) { }

// Patch screen transitions
[HarmonyPatch(typeof(UIScreenBase), "Show")]
[HarmonyPostfix]
static void AnnounceScreenChange(UIScreenBase __instance) { }

// Patch selection changes
[HarmonyPatch(typeof(STESelectableWidgetBase), "Select")]
[HarmonyPostfix]
static void AnnounceSelection(STESelectableWidgetBase __instance) { }
```

## Next Steps
1. Review existing `UIPatches.cs` implementation
2. Add patches for priority classes
3. Test with SRAL screen reader
4. Iterate based on user feedback
