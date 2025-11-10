# Testing Checklist

Copy this checklist and mark items as you complete them:

## Pre-Testing Setup

- [ ] Game is installed via Steam
- [ ] MelonLoader is installed (`.\scripts\setup-melonloader.ps1`)
- [ ] Game has been launched once (generates MelonLoader assemblies)
- [ ] Project references are updated (`.\scripts\update-references.ps1`)
- [ ] SRAL.dll is built (see `docs\building-sral.md`)
- [ ] Screen reader is running (NVDA/JAWS/Narrator)

## Build and Deploy

- [ ] Run `.\scripts\deploy-mod.ps1`
- [ ] Build succeeded (no errors)
- [ ] DLL copied to game `Mods/` folder
- [ ] `SRAL.dll` is in `Mods/` folder

## Enable Debug Logging

- [ ] Launch game once
- [ ] Close game
- [ ] Open `<GameFolder>\UserData\MelonPreferences.cfg` in text editor
- [ ] Find `[CKAccessibility]` section
- [ ] Change `DebugTextExtraction = false` to `DebugTextExtraction = true`
- [ ] Save and close file

## In-Game Testing

- [ ] Launch game
- [ ] Screen reader announces "Mod ready" (SRAL initialized)
- [ ] Reach main menu
- [ ] Press Tab key multiple times
- [ ] Hear button announcements (record what you hear below)

**What I heard**:
```
Button 1: _______________
Button 2: _______________
Button 3: _______________
Button 4: _______________
Button 5: _______________
```

**Did it work?** (Circle one):  YES  /  NO  /  PARTIALLY

## Log Analysis

- [ ] Open `<GameFolder>\MelonLoader\Latest.log` with screen reader
- [ ] Search for `[Accessibility]` - Verify mod loaded
- [ ] Search for `SRAL initialized` - Verify screen reader integration works
- [ ] Search for `[GetButtonText]` - Find text extraction attempts
- [ ] Copy 10-15 relevant log lines (paste below)

**Relevant log excerpt**:
```
(Paste here)
```

**Which strategy succeeded?** (Check one):
- [ ] Strategy 1: "Direct TMP access"
- [ ] Strategy 2: "Reflection TMP text"
- [ ] Strategy 3: "STETextBlock field"
- [ ] None: "Using fallback GO name"

## Optional: UnityExplorer Inspection

Only if text extraction failed completely.

- [ ] Run `.\scripts\install-unityexplorer.ps1`
- [ ] Launch game
- [ ] Press F7 (opens UnityExplorer)
- [ ] Navigate to C# Console tab (may need sighted help)
- [ ] Type: `var btn = GameObject.Find("MainMenu_2"); UnityEngine.Debug.Log(btn.name);`
- [ ] Check `Latest.log` for output
- [ ] (Ask for more queries to run if needed)

## Report to AI Assistant

Paste this completed checklist or summarize:

**Summary**:
- Text extraction worked: YES / NO
- Announcements heard: (examples)
- Strategy that worked: (1, 2, 3, or none)
- Log excerpt: (paste above section)
- Errors encountered: (any red ERROR lines)

## Next Steps (AI Will Guide)

Based on your findings, next actions will be one of:

- ✅ **If it worked**: Disable debug logging, expand to more UI elements
- ⚠️ **If partially worked**: Optimize strategy order for performance
- ❌ **If failed**: Deep inspection with UnityExplorer, code adjustments

---

## Quick Command Reference

```powershell
# Build and deploy
.\scripts\deploy-mod.ps1

# Install UnityExplorer (optional)
.\scripts\install-unityexplorer.ps1

# Update references (after game updates)
.\scripts\update-references.ps1

# View logs
Get-Content "<GameFolder>\MelonLoader\Latest.log" -Tail 50

# Search logs
Select-String -Path "<GameFolder>\MelonLoader\Latest.log" -Pattern "[GetButtonText]"
```

---

**Time estimate**: 15-20 minutes total. Good luck! 🚀
