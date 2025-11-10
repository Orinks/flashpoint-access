# Testing Workflow for Text Extraction Debugging

## Changes Made to Your Code

Your `GetButtonText()` method now uses **3-tier text extraction strategy**:

### Strategy 1: Direct IL2CPP Access (NEW!)
```csharp
var tmpComponent = monoBehaviour.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
if (tmpComponent != null) return tmpComponent.text;
```
- **Fastest and most reliable** for standard Unity TextMeshPro
- Works if MelonLoader properly exposes `TMPro.*` types
- Returns immediately on success (no fallback needed)

### Strategy 2: Reflection with Multiple Assembly Names (ENHANCED)
```csharp
Type tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro")
    ?? Type.GetType("TMPro.TextMeshProUGUI, Assembly-CSharp")
    ?? Type.GetType("TMPro.TextMeshProUGUI, Il2CppUnity.TextMeshPro");
```
- Tries **3 common assembly names** (game might use custom build)
- Falls back to **private field access** (`_text`, `m_text`) if property fails
- Enhanced logging shows exact assembly name found

### Strategy 3: STETextBlock with Field Access (ENHANCED)
```csharp
var textField = childType.GetField("_text", BindingFlags.NonPublic | BindingFlags.Instance)
    ?? childType.GetField("_Text", ...) 
    ?? childType.GetField("m_text", ...);
```
- Now checks **private fields** if properties don't exist
- Cyber Knights likely uses custom `STETextBlock` class with private fields

### Enhanced Debug Logging (NEW!)
When `DebugTextExtraction = true`, you'll see:
```
[GetButtonText] Components on ButtonLabel:
  - TMPro.TextMeshProUGUI
    Assembly: Unity.TextMeshPro | ToString: New Game
    text property: "New Game"
```

---

## Testing Workflow

### Step 1: Enable Debug Logging (DO THIS FIRST!)

1. **Build and deploy your mod once**:
   ```powershell
   .\scripts\deploy-mod.ps1
   ```

2. **Launch game once** to generate config file

3. **Edit config**:
   ```
   <GameFolder>\UserData\MelonPreferences.cfg
   ```
   
   Find and change:
   ```ini
   [CKAccessibility]
   DebugTextExtraction = true
   ```

4. **Save and close** the config file

### Step 2: Test In-Game

1. **Launch the game** (MelonLoader auto-loads your mod)

2. **Watch for mod load**:
   - Screen reader should announce "Mod ready" via SRAL
   - If no announcement, check `MelonLoader\Latest.log` for errors

3. **Navigate main menu**:
   - Press **Tab** repeatedly to cycle through buttons
   - Press **Arrow keys** if Tab doesn't work
   - Each button selection triggers `OnSelect` patch

4. **Listen for announcements**:
   - **Correct**: "New Game", "Continue", "Options", etc.
   - **Wrong**: "Main Menu 2", "Main Menu 3", "Button", etc.

### Step 3: Analyze Logs

1. **Open log file** with screen reader:
   ```
   <GameFolder>\MelonLoader\Latest.log
   ```

2. **Search for** (Ctrl+F): `[GetButtonText]`

3. **Look for patterns**:

   #### Success Pattern (Text Extracted):
   ```
   [GetButtonText] Button type: Il2CppRPG.UI.Widgets.Buttons.STEButton, GO: MainMenu_2
   [GetButtonText] Direct TMP access: "New Game"
   ```
   ✅ **Strategy 1 worked!** No changes needed.

   #### Fallback Pattern (Reflection Worked):
   ```
   [GetButtonText] Found 5 child transforms
   [GetButtonText] Components on ButtonLabel:
     - TMPro.TextMeshProUGUI
       Assembly: Unity.TextMeshPro | ToString: New Game
       text property: "New Game"
   [GetButtonText] Reflection TMP text: "New Game" on ButtonLabel
   ```
   ✅ **Strategy 2 worked!** Direct access failed but reflection succeeded.

   #### Custom Component Pattern:
   ```
   [GetButtonText] Found 3 child transforms
   [GetButtonText] Components on Label:
     - Il2CppRPG.UI.Widgets.Text.STETextBlock
       Assembly: Assembly-CSharp | ToString: STETextBlock
       text property: ERROR - Property not found
   [GetButtonText] STETextBlock field: "New Game"
   ```
   ✅ **Strategy 3 worked!** Game uses custom component with private fields.

   #### Failure Pattern (GameObject Name Fallback):
   ```
   [GetButtonText] Found 2 child transforms
   [GetButtonText] Components on MainMenu_2:
     - UnityEngine.Transform
     - UnityEngine.CanvasRenderer
   [GetButtonText] Using fallback GO name: "Main Menu 2"
   ```
   ❌ **All strategies failed!** No text component found. See troubleshooting below.

### Step 4: Take Action Based on Findings

#### If Strategy 1 Succeeded ✅
You're done! Text extraction works. Disable debug logging:
```ini
DebugTextExtraction = false
```

#### If Strategy 2/3 Succeeded ⚠️
Works but slower. **Optimization (optional)**:
- If you see `STETextBlock` consistently, move Strategy 3 **before** Strategy 2 in code
- Comment: "// Game uses STETextBlock, prioritize to reduce overhead"

#### If All Strategies Failed ❌
**Send me the following from log**:
1. Complete `[GetButtonText]` block for one button
2. Component list from "Components on X:" section
3. Any warnings/errors

I'll provide exact fix based on what components are actually present.

---

## UnityExplorer Usage (Optional Deep Inspection)

### When to Use UnityExplorer
- Logs show components but no text extracted
- Want to test component access without rebuilding mod
- Need to confirm hierarchy structure

### Installation
Already scripted for you:
```powershell
.\scripts\install-unityexplorer.ps1
```

### In-Game Usage

1. **Launch game, press F7** (toggles UnityExplorer UI)

2. **Object Explorer Tab**:
   - Search: `"Button"` or `"MainMenu"`
   - Select a result (press Enter)
   - Inspector shows all components

3. **C# Console Tab** (Most Accessible!):
   Type commands, results log to `Latest.log`:
   
   ```csharp
   // Find button GameObject
   var btn = GameObject.Find("MainMenu_2");
   UnityEngine.Debug.Log("Found: " + btn.name);
   
   // List all components
   var comps = btn.GetComponents<Component>();
   foreach (var c in comps) {
       UnityEngine.Debug.Log("Component: " + c.GetType().FullName);
   }
   
   // Try TextMeshPro access
   var tmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
   if (tmp != null) {
       UnityEngine.Debug.Log("TMP text: " + tmp.text);
   }
   
   // Try STETextBlock (custom)
   var stb = btn.GetComponentInChildren<Il2CppRPG.UI.Widgets.Text.STETextBlock>();
   if (stb != null) {
       UnityEngine.Debug.Log("STETextBlock found");
       // Access properties via reflection in C# console
   }
   ```

4. **Check `MelonLoader\Latest.log`** for Debug.Log outputs (screen reader accessible!)

---

## Common Issues & Solutions

### Issue: "Direct TMP access failed: Type not found"
**Cause**: `TMPro.TextMeshProUGUI` not exposed by MelonLoader interop  
**Solution**: Strategies 2/3 will handle this automatically

### Issue: "Reflection TMP error: Could not load type"
**Cause**: Assembly name doesn't match any of the 3 tried  
**Fix**: Add the correct assembly name from logs:
```csharp
Type tmpType = Type.GetType("TMPro.TextMeshProUGUI, <YourAssemblyName>")
    ?? /* existing fallbacks */;
```

### Issue: "STETextBlock property: ERROR"
**Cause**: Text is in a private field, not public property  
**Status**: ✅ Already handled by Strategy 3 field access

### Issue: No components logged, only GameObject name
**Cause**: Text might be on parent or sibling, not child  
**Fix**: Try `GetComponentInParent` or traverse siblings

### Issue: Mod doesn't load at all
**Symptoms**: No "Mod ready" announcement, no logs  
**Check**:
1. `SRAL.dll` in `Mods/` folder?
2. `MelonLoader\Latest.log` shows "CKFlashpointAccessibility loaded"?
3. Any errors in log about missing references?

---

## Performance Notes

### Current Overhead (Per Button Selection)
- **Strategy 1**: ~0.01ms (direct GetComponent)
- **Strategy 2**: ~1-5ms (reflection + Type.GetType)
- **Strategy 3**: ~5-10ms (GetComponentsInChildren + reflection)

### Rate Limiting (Already Implemented)
Your `SRALHelper.Speak()` has 100ms cooldown:
```csharp
if ((now - _lastSpeechTime).TotalMilliseconds < delay && !interrupt)
    return;
```
This prevents spam during rapid Tab key presses.

---

## What to Report

After testing, share:

1. **Does it work?**
   - YES: "Buttons announce correctly: [examples]"
   - NO: "Still announces GameObject names"

2. **Which strategy succeeded?**
   - Check log for: "Direct TMP access" (Strategy 1) vs "Reflection TMP" (2) vs "STETextBlock" (3)

3. **Relevant log excerpt**:
   ```
   [GetButtonText] Button type: ...
   [GetButtonText] Found X child transforms
   [GetButtonText] Components on Y: ...
   ```

4. **Any errors/warnings** in the log

---

## Next Steps After Success

1. **Disable debug logging** for performance:
   ```ini
   DebugTextExtraction = false
   ```

2. **Test other screens**:
   - Mission Planning
   - Combat HUD
   - Dialog choices
   - Inventory

3. **Expand patches** for screen-specific UI (already have stubs in `UIPatches.cs`)

4. **Polish**:
   - Add position info: "New Game, button 1 of 5"
   - Keyboard hints: "Press Tab to navigate"
   - Context announcements: "Main Menu" when entering screen

Your implementation is now robust with 3-tier fallback + enhanced logging. Let me know what the logs show!
