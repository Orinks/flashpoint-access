# Accessibility Implementation Notes

## Overview
Successfully implemented Harmony patches for Cyber Knights: Flashpoint accessibility using the SRAL screen reader library.

## Implementation Date
November 10, 2025

## Components Implemented

### 1. Core UI Patches (`UIPatches.cs`)

#### STEButton Patches
- **OnPointerClick**: Announces "Button activated: [text]" when clicked
- **OnPointerEnter**: Announces "Button: [text]" on hover
- Extracts text from STETextBlock children or TextMeshPro components

#### STETextBlock Patches
- **SetText**: Announces text whenever it's updated
- Primary text display component used throughout the game

#### UIScreenBase Patches
- **Show**: Announces screen name when transitioning
- Formats screen name (removes "Screen_" prefix and underscores)

#### STESelectableWidgetBase Patches
- **Select**: Announces "Selected: [text]" for any selectable element

#### STEDialogAnswerButton Patches
- **OnPointerEnter**: Announces "Dialog option: [text]" for conversation choices

### 2. Screen-Specific Patches (`ScreenPatches.cs`)
Dedicated announcements for major game screens:
- Mission Planning
- Roster management
- Loadout/Equipment
- Training
- Cyberdeck

### 3. Text Input Patches (`TextInputPatches.cs`)
- **OnSelect**: Announces "Text input field" when focused
- **OnValueChanged**: Speaks each character as it's typed

### 4. List Navigation Patches (`ListPatches.cs`)
- **STETextItem.OnPointerEnter**: Announces list item text on hover

### 5. Input Feedback Patches (`InputPatches.cs`)
- Monitors keyboard input for navigation feedback
- Currently announces Escape key presses

## Text Extraction Strategy

The patches use a multi-tier approach to extract text:

1. **STETextBlock components** - Primary method using reflection to access internal text fields
2. **TextMeshPro (TMPro.TextMeshProUGUI)** - Common Unity text component
3. **GameObject name** - Fallback when no text component is found

```csharp
// Example extraction logic
var textBlock = component.GetComponentInChildren<STETextBlock>();
var textField = textBlock.GetType().GetField("text", BindingFlags);
var value = textField.GetValue(textBlock);
```

## Configuration Options

Available in MelonLoader preferences:
- `Enabled`: Master toggle for accessibility features
- `AnnounceMenuItems`: Enable/disable menu item announcements
- `AnnounceButtons`: Enable/disable button announcements
- `SpeechDelay`: Milliseconds between announcements (default: 100ms)
- `InterruptPrevious`: Whether new speech interrupts current speech

## SRAL Integration

SRAL initialization priority:
1. UIA (UI Automation) - Best for Windows
2. SAPI (Windows Speech API)
3. NVDA (direct)
4. JAWS (direct)

The mod automatically selects the best available engine at startup.

## Build Requirements

### Dependencies
- MelonLoader (for mod loading)
- HarmonyLib (for runtime patching)
- Il2CppInterop.Runtime (for IL2CPP game compatibility)
- UnityEngine libraries
- SRAL.dll (screen reader library)

### Project Structure
```
CKFlashpointAccessibility/
├── Plugin.cs              - Main mod entry point
├── SRAL.cs               - P/Invoke wrapper for SRAL
└── Patches/
    └── UIPatches.cs      - All Harmony patches
```

## Testing Strategy

### Phase 1: Basic Functionality
- [ ] Button clicks are announced
- [ ] Button hover works
- [ ] Screen transitions are spoken
- [ ] Text updates are announced

### Phase 2: Navigation
- [ ] List items announce on hover
- [ ] Selection changes are spoken
- [ ] Text input feedback works

### Phase 3: Screen-Specific
- [ ] Mission planning screen accessible
- [ ] Roster management works
- [ ] Loadout editing accessible
- [ ] Training interface works

### Phase 4: Dialogs
- [ ] Dialog choices are announced
- [ ] Character names are spoken
- [ ] Dialog text is accessible

## Known Considerations

1. **Reflection Usage**: Some patches use reflection to access private fields. This may need adjustment if the game updates.

2. **Rate Limiting**: Speech delay prevents announcement spam. Adjust `SpeechDelay` config if needed.

3. **IL2CPP Compatibility**: Game uses IL2CPP, requiring Il2CppInterop for proper type handling.

4. **Performance**: Harmony patches add minimal overhead. SRAL speech is asynchronous.

## Next Steps

1. **Deploy and Test**: Copy compiled mod to MelonLoader mods folder
2. **User Testing**: Get feedback from screen reader users
3. **Refinement**: Adjust announcement text based on feedback
4. **Additional Patches**: Add more specific patches for combat, inventory, etc.
5. **Keyboard Navigation**: Implement custom navigation helpers if needed

## Deployment

Build the project and copy to:
```
C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint\Mods\
```

Ensure SRAL.dll is in the game directory or Mods folder.

## Support Resources

- IL2CPP Dump: `dumped/dump.cs` (861,338 lines analyzed)
- UI Class List: `UI-CLASSES-TO-PATCH.md`
- SRAL Documentation: `tools/SRAL/README.md`
- Build Scripts: `scripts/deploy-mod.ps1`

## Credits

- **Developer**: Joshua
- **SRAL Library**: blindgoofball (GitHub)
- **MelonLoader**: MelonLoader team
- **Game**: Cyber Knights: Flashpoint by Trese Brothers
