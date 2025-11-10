# BepInEx vs MelonLoader: Clarification

## ❌ Don't Switch to BepInEx

You received troubleshooting instructions written for a **BepInEx + Tolk** setup, but your project uses **MelonLoader + SRAL** (already migrated for accessibility reasons).

### Why Those Instructions Don't Apply

| Their Instructions | Your Reality |
|-------------------|--------------|
| Install BepInEx | ✅ Already using MelonLoader (better for blind devs) |
| Use Tolk library | ✅ Using SRAL (more flexible screen reader support) |
| Manual IL2CPP dump | ✅ MelonLoader auto-generates assemblies |
| UnityExplorer for BepInEx | ❌ Need MelonLoader version (different DLL) |

### Key Differences

**BepInEx:**
- Console output NOT screen reader friendly
- Requires manual IL2CPP dumping with Il2CppDumper
- More common for modding (larger community)

**MelonLoader:**
- ✅ Console works with NVDA/JAWS
- ✅ Auto-generates IL2CPP assemblies on first launch
- ✅ Better accessibility for blind developers
- ❌ Smaller community (but growing)

You chose MelonLoader for good reasons (documented in `MELONLOADER-MIGRATION.md`).

---

## What IS Relevant from Those Instructions

### 1. Debug Logging Approach ✅
**Their concept**: Enable logging to see component types at runtime  
**Your implementation**: Already built-in! Just set `DebugTextExtraction = true` in config

### 2. UnityExplorer for Runtime Inspection ✅
**Their concept**: Use UnityExplorer to inspect UI GameObjects in-game  
**Your implementation**: Use MelonLoader version via `scripts/install-unityexplorer.ps1`

### 3. IL2CPP Type Resolution Patterns ✅
**Their concept**: Use `Il2CppType.From()` and assembly-qualified names  
**Your implementation**: Already doing this in `UIPatches.cs` lines 454-470

### 4. Reflection for Private Fields ✅
**Their concept**: Text might be in `_text` private field, not public `text` property  
**Your implementation**: Good suggestion! Can add to `GetButtonText()` if needed

---

## What You Should Actually Do

### Step 1: Enable Existing Debug Logging
Edit: `<GameFolder>\UserData\MelonPreferences.cfg`
```ini
[CKAccessibility]
DebugTextExtraction = true
```

### Step 2: Test and Capture Logs
1. Launch game
2. Navigate main menu (Tab/Arrow keys)
3. Select buttons
4. Read `MelonLoader\Latest.log` with screen reader

Look for lines like:
```
[GetButtonText] Button type: Il2CppRPG.UI.Widgets.Buttons.STEButton
[GetButtonText] Found 5 child transforms
[GetButtonText] Components on ButtonLabel:
  - TMPro.TextMeshProUGUI | ToString: New Game
```

### Step 3: (Optional) Install UnityExplorer
If logs aren't clear enough:
```powershell
.\scripts\install-unityexplorer.ps1
```

Use C# Console to query objects (results go to `Latest.log` for screen reader access).

### Step 4: Fix Code Based on Findings
Once you know the exact component type/property name, update `UIPatches.cs`:
- Adjust `Type.GetType()` assembly qualification
- Check for private `_text` fields if property fails
- Prioritize STETextBlock if that's what game uses

See `DEBUGGING-TEXT-EXTRACTION.md` for detailed fix patterns.

---

## UnityExplorer: CLI Question

### Short Answer: No CLI Exists

UnityExplorer is an **in-game GUI overlay** (press F7). It does NOT have a command-line interface.

### Accessibility Workarounds

**Option A: C# Console (Screen Reader Accessible)**
1. Open UnityExplorer in-game (F7)
2. Go to C# Console tab (may need sighted help to find)
3. Type code to inspect objects:
   ```csharp
   var btn = GameObject.Find("MainMenu_2");
   UnityEngine.Debug.Log(btn.GetComponent<Component>().GetType());
   ```
4. Results appear in `MelonLoader\Latest.log` (read with NVDA/JAWS)

**Option B: Mouse Inspection (Requires Sighted Assistance)**
- Use Mouse Inspect mode to hover over UI elements
- Have someone screenshot the component list
- You can read component names/types from screenshot OCR or sighted description

**Option C: Rely on Your Built-In Debug Logging**
- Your `GetButtonText()` already logs ALL child transforms and components
- This might be sufficient without needing UnityExplorer!

---

## Summary

| Should I... | Answer |
|-------------|--------|
| Switch to BepInEx? | ❌ No, stay with MelonLoader (better accessibility) |
| Use Tolk instead of SRAL? | ❌ No, SRAL supports more screen readers |
| Install UnityExplorer? | ⚠️ Optional - your debug logging might be enough |
| Follow their IL2CPP patterns? | ✅ Yes, the concepts apply (but you already do this) |
| Enable debug logging? | ✅ Yes! Do this first before anything else |

---

## Next Action Plan

1. ✅ **Enable `DebugTextExtraction = true`** in config
2. ✅ **Test in-game** and capture logs
3. ✅ **Read `Latest.log`** to see what component types exist
4. ✅ **Update `GetButtonText()`** based on findings
5. ⚠️ **Install UnityExplorer** only if logs are insufficient

See `DEBUGGING-TEXT-EXTRACTION.md` for step-by-step instructions tailored to YOUR MelonLoader setup.

---

## Contact Points

If logs show:
- **No components found** → Check mod is loading (search log for "CK Flashpoint Accessibility")
- **Components found but no text types** → Game might use fully custom UI (share log excerpt)
- **Text types found but extraction fails** → Assembly qualification issue (share error message)
- **Text extracted but wrong content** → Might need to prioritize different component (share candidates list from log)

The instructions you received are 80% applicable, just written for the wrong mod loader!
