# Debugging Text Extraction with MelonLoader + SRAL

## Your Current Setup
- **Mod Loader**: MelonLoader (NOT BepInEx)
- **Screen Reader Library**: SRAL (NOT Tolk)
- **Issue**: Buttons announce GameObject names (e.g., "Main Menu 2/3/4") instead of actual text

## Why This Happens
IL2CPP wraps Unity types in `Il2Cpp*` prefixes. Your code already handles this, but the **specific type name** might be wrong for this game.

---

## Step 1: Enable Your Existing Debug Logging

Your mod already has debug logging built-in!

### Edit MelonPreferences Config
After running your mod once, edit:
```
<GameFolder>\UserData\MelonPreferences.cfg
```

Find the `[CKAccessibility]` section and change:
```ini
[CKAccessibility]
DebugTextExtraction = true
```

### What This Does
Your `GetButtonText()` method will log:
- Button type name
- GameObject name
- All child components found
- TextMeshPro/STETextBlock detection attempts
- Final extracted text

---

## Step 2: Test In-Game and Capture Logs

1. **Launch the game** (your mod auto-loads via MelonLoader)
2. **Navigate to main menu** using Tab/Arrow keys
3. **Select each button** (your OnSelect patch fires)
4. **Check logs**:
   ```
   <GameFolder>\MelonLoader\Latest.log
   ```

### What to Look For
Search for lines like:
```
[GetButtonText] Button type: Il2CppRPG.UI.Widgets.Buttons.STEButton, GO: MainMenu_2
[GetButtonText] Found X child transforms
[GetButtonText] Components on ButtonLabel:
  - TMPro.TextMeshProUGUI (IL2CPP: ...) | ToString: New Game
  - UnityEngine.Transform | ...
```

**Key Questions:**
- What's the **exact type name** of text components? (TMPro.TextMeshProUGUI? STETextBlock?)
- Which **assembly** are they in? (Unity.TextMeshPro? Assembly-CSharp?)
- Do they have a `text` property?

---

## Step 3: Install UnityExplorer (Optional, More Detailed Inspection)

UnityExplorer has **no CLI**, but works in-game with MelonLoader.

### Installation
1. **Download**: https://github.com/sinai-dev/UnityExplorer/releases
   - Get `UnityExplorer.MelonLoader.Il2Cpp.zip` (NOT BepInEx version!)
2. **Extract** to `<GameFolder>\Mods\` (NOT BepInEx\plugins)
3. **Launch game** - Press **F7** to toggle UnityExplorer

### Using UnityExplorer (Without Vision)
UnityExplorer's GUI is **not screen reader accessible** by default, but you can:

#### Option A: Mouse Inspection (Requires Sighted Assistance)
1. Open UnityExplorer (F7)
2. Go to **Inspector** tab → **Mouse Inspect** dropdown → Select "UI"
3. Hover over main menu buttons in-game
4. UnityExplorer shows all components on that GameObject
5. Screenshot/record component names for you

#### Option B: C# Console (Accessible via Logs)
1. Open UnityExplorer (F7)
2. Go to **C# Console** tab
3. Type commands like:
   ```csharp
   var button = GameObject.Find("MainMenu_2");
   var comps = button.GetComponents<Component>();
   foreach (var c in comps) {
       UnityEngine.Debug.Log($"{c.GetType().FullName}");
   }
   ```
4. Results appear in `Latest.log` (you can read with screen reader)

#### Option C: Object Explorer Search
1. Go to **Object Explorer** tab
2. Search for "Button" or "MainMenu"
3. Select a GameObject (press Enter on search result)
4. Inspector shows all components - logs appear in `Latest.log`

---

## Step 4: Fix GetButtonText Based on Findings

### Scenario A: TextMeshProUGUI Found, Wrong Type Resolution
**Your logs show**: `Type.GetType("TMPro.TextMeshProUGUI")` returns `null`

**Likely Fix**: Assembly qualification is wrong. Update line 454-458 in `UIPatches.cs`:

```csharp
// Try multiple assembly names (game might use custom TMP assembly)
Type tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro") 
    ?? Type.GetType("TMPro.TextMeshProUGUI, Assembly-CSharp")
    ?? Type.GetType("TMPro.TextMeshProUGUI, Il2CppTMPro");
```

### Scenario B: Game Uses Custom Text Component
**Your logs show**: No TextMeshProUGUI, but components like `Il2CppRPG.UI.Widgets.Text.STETextBlock`

**Fix**: Prioritize STETextBlock (you already have this as fallback around line 493):
```csharp
// Move STETextBlock check BEFORE TextMeshPro check in GetButtonText
var children = monoBehaviour.GetComponentsInChildren<MonoBehaviour>(true);
foreach (var child in children)
{
    if (child.GetType().Name == "STETextBlock" || child.GetType().Name.Contains("STEText"))
    {
        // Try multiple property names
        var textProp = child.GetType().GetProperty("text") 
            ?? child.GetType().GetProperty("Text")
            ?? child.GetType().GetProperty("_text", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (textProp != null)
        {
            var value = textProp.GetValue(child);
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                return value.ToString(); // Return immediately
            }
        }
    }
}
```

### Scenario C: Text is in Private Fields
**Your logs show**: Components exist but `text` property returns empty

**Fix**: Use reflection to access private `_text` or `m_text` fields:
```csharp
// After property check fails, try fields
var textField = childType.GetField("_text", BindingFlags.NonPublic | BindingFlags.Instance)
    ?? childType.GetField("m_text", BindingFlags.NonPublic | BindingFlags.Instance)
    ?? childType.GetField("_Text", BindingFlags.NonPublic | BindingFlags.Instance);

if (textField != null)
{
    var value = textField.GetValue(child);
    if (value != null) candidates.Add(value.ToString());
}
```

---

## Step 5: Test Iteration

1. **Make code change** based on findings
2. **Build**: `.\scripts\deploy-mod.ps1`
3. **Launch game** and test main menu
4. **Check logs** - repeat until buttons announce correctly

---

## Common IL2CPP Gotchas

### ❌ Wrong: Direct Type.GetType()
```csharp
Type tmpType = Type.GetType("TextMeshProUGUI"); // Fails in IL2CPP!
```

### ✅ Right: Assembly-Qualified + Il2CppType.From()
```csharp
Type tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
Il2CppSystem.Type il2cppType = Il2CppType.From(tmpType);
Component comp = go.GetComponent(il2cppType);
```

### ❌ Wrong: Generic GetComponent<T>()
```csharp
var tmp = go.GetComponent<TextMeshProUGUI>(); // Crashes!
```

### ✅ Right: Non-generic with Il2Cpp Type
```csharp
Component comp = go.GetComponent(Il2CppType.From(typeof(TextMeshProUGUI)));
```

---

## When to Contact Me

If after Step 2 (debug logging), you see:
- **No components logged** → Code might not be running (check mod loaded in logs)
- **Components logged but no text types** → Game uses fully custom UI (needs deeper analysis)
- **TextMeshPro found but reflection fails** → Share exact error message
- **Text extracted but wrong content** → Share log excerpt showing candidates

Paste relevant `Latest.log` sections and I'll provide exact fix!

---

## Why Not Switch to BepInEx?

The instructions you received assume BepInEx, but you **already migrated to MelonLoader** because:
- ✅ MelonLoader console output works with NVDA/JAWS
- ✅ Better accessibility for blind developers
- ✅ Auto-generates IL2CPP assemblies (no manual dump needed)
- ✅ Harmony included out-of-box

**Stick with MelonLoader** - your issue is just finding the right text component type, not the mod loader.

---

## Quick Reference: Your Existing Code Structure

| File | Current Implementation | 
|------|------------------------|
| `Plugin.cs` | MelonMod with SRAL initialization, config system |
| `SRAL.cs` | P/Invoke wrapper for native SRAL.dll (NVDA/JAWS/SAPI) |
| `UIPatches.cs` | Harmony patches for STEButton.OnSelect, GetButtonText helper |
| `TextExtractionUtils.cs` | Label aggregation/deduplication logic |

Your architecture is **solid** - just need runtime debugging to find the exact component type name!
