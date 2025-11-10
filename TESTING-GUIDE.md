# Testing Guide: CK Flashpoint Accessibility Mod

## Build Status
✅ **Build Successful!** - CKFlashpointAccessibility.dll compiled

## Installation

1. Copy the built DLL to the MelonLoader Mods folder:
```powershell
Copy-Item ".\CKFlashpointAccessibility\bin\Debug\net6.0\CKFlashpointAccessibility.dll" `
    -Destination "C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint\Mods\"
```

2. Ensure SRAL.dll is also in the Mods folder or game root directory

## Expected Behavior When Testing

### On Game Launch

**Expected Announcement:**
- "Cyber Knights Flashpoint accessibility mod loaded successfully!"
- This confirms SRAL initialized and the mod is active

**MelonLoader Console Should Show:**
- "SRAL screen reader library initialized successfully."
- "SRAL initialized with engine: [ENGINE_NAME]"
- "Engine features: [FEATURES]"
- "Patched: STEButton.OnPointerClick"
- "Patched: STEButton.OnPointerEnter"
- "Patched: STETextBlock.SetText"
- etc. for each successful patch

### During Gameplay

#### 1. **KEYBOARD NAVIGATION (Primary Focus for Blind Users)**

**When navigating with Tab/Arrow keys:**
- Screen reader announces the currently selected button/option
- Example: Press Tab → hear "Start Game"
- Press Tab again → hear "Continue"
- Press Tab again → hear "Options"

**Key Navigation:**
- `Tab` / `Shift+Tab` - Navigate between buttons and UI elements
- `Arrow Keys` - Navigate within lists and menus
- `Enter` / `Space` - Activate selected button
- `Escape` - Go back/cancel

#### 2. Button Selection & Activation

**When selecting a button (via Tab/Arrows):**
- Screen reader announces: "[button text]"
- Clean, simple announcement of what's selected

**When clicking/activating a button:**
- Screen reader announces: "Button activated: [button text]"
- Confirms the action was performed

#### 2. Screen Transitions

**When entering a new screen:**
- Screen reader announces: "Entering [Screen Name] screen"
- Examples:
  - "Entering Mission Planning screen"
  - "Entering Roster screen"
  - "Entering Loadout screen"
  - "Entering Training screen"
  - "Entering Cyberdeck screen"

#### 3. Text Updates

**When text changes on screen:**
- Screen reader speaks the new text
- This includes stat updates, descriptions, tooltips

#### 4. Dialog System

**When navigating dialog choices (Arrow keys):**
- Screen reader announces: "[option text]"
- Navigate conversation choices with arrow keys
- Clean announcement of each dialog option
#### 5. List Navigation

**When navigating lists (Arrow keys):**
- Screen reader announces the item text
- Use Up/Down arrows to navigate lists
- Character lists, item lists, mission lists, inventory
- Useful for character lists, item lists, mission lists

#### 6. Text Input Fields

**When focusing a text field:**
- Screen reader announces: "Text input field"

**When typing:**
- Each character is spoken as you type
- Provides typing feedback

#### 7. Selectable Elements

**When selecting UI elements (Tab/Arrow navigation):**
- Screen reader announces: "[element text]"
- Works with keyboard navigation, not mouse hover
- Announces menu items, list items, character names, equipment, etc.

### Configuration Options

Configuration file location:
`C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint\UserData\MelonPreferences.cfg`

**Available Settings:**
```ini
[CKAccessibility]
Enabled = true                 # Master toggle
AnnounceMenuItems = true       # Enable menu item announcements
AnnounceButtons = true         # Enable button announcements  
SpeechDelay = 100              # Delay between announcements (ms)
InterruptPrevious = true       # New speech interrupts current
```

### Troubleshooting

#### If No Speech Output:

1. **Check SRAL Initialized:**
   - Look in MelonLoader console for "SRAL initialized" message
   - If missing, SRAL.dll may not be found

2. **Check Screen Reader Running:**
   - NVDA, JAWS, or Windows Narrator should be running
   - SRAL supports: UIA, SAPI, NVDA, JAWS

3. **Check Patches Applied:**
   - Console should show "Patched: ..." messages
   - If patches failed, check for Assembly-CSharp loading errors

#### If Some Patches Don't Work:

- Some game classes may not exist or have different method names
- Check MelonLoader console for "Type not found" or "Method not found" warnings
- These are non-critical and mean that specific patch was skipped

#### If Speech is Too Frequent:

- Increase `SpeechDelay` in config (default: 100ms)
- Set `InterruptPrevious = false` to let announcements finish

#### If Speech is Too Quiet:

- SRAL uses the system screen reader's volume
- Adjust volume in NVDA/JAWS settings or Windows Narrator settings

### Performance Notes

- **Minimal Impact:** Harmony patches add negligible overhead
- **Async Speech:** SRAL speech is non-blocking
### Testing Checklist

- [ ] Mod loads successfully (hear announcement)
- [ ] **Tab key navigation announces buttons/options**
- [ ] **Arrow keys navigate lists with announcements**
- [ ] **Enter key activates selected button with confirmation**
- [ ] Screen transitions are announced
- [ ] Text updates are spoken
- [ ] Dialog options announce when selected (not hover)
- [ ] Menu items announce when selected with keyboard
- [ ] Text input provides typing feedback
- [ ] No game crashes or freezes
- [ ] Performance is acceptable
- [ ] **Can navigate entire UI with keyboard only**g feedback
- [ ] No game crashes or freezes
- [ ] Performance is acceptable

### Known Limitations

1. **Runtime Patching:** Some classes may not be found if game structure changes
2. **Text Extraction:** Relies on reflection - may miss some custom text components
3. **Combat UI:** Combat-specific UI may need additional patches
4. **Tutorial System:** Tutorial overlays may need special handling

### Next Steps After Testing

1. **Report What Works:** Note which announcements are helpful
2. **Report What's Missing:** Identify UI elements that aren't announced
3. **Report Problems:** Any crashes, errors, or confusing announcements
4. **Suggest Improvements:** Better announcement text or timing

### Debug Mode

To see detailed logging:
1. Open MelonLoader console (press F1 in game)
2. Watch for error messages prefixed with the mod name
3. Check for "Error in [PatchName]" messages

### Success Criteria

The mod is working correctly if:
- ✅ Game launches without crashes
- ✅ Screen reader announces at least some UI elements
- ✅ Button clicks are announced
- ✅ Screen transitions are announced
- ✅ No significant performance degradation

Good luck testing! 🎮🔊
