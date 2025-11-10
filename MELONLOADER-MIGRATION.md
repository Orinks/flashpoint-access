# MelonLoader Migration Complete ✅

## What Changed

The project has been **migrated from BepInEx to MelonLoader** for better screen reader accessibility!

### Key Changes

1. **Mod Structure**
   - Changed from `BepInPlugin` → `MelonMod`
   - Config system: `BepInEx.Configuration` → `MelonPreferences`
   - Logging: `ManualLogSource` → `MelonLogger.Instance`

2. **Setup Process**
   - ~~`setup-bepinex.ps1`~~ → `setup-melonloader.ps1`
   - ~~`dump-il2cpp.ps1`~~ → No longer needed! MelonLoader auto-generates
   - Deployment: `BepInEx/plugins/` → `Mods/`

3. **References**
   - No NuGet packages needed
   - References local MelonLoader.dll from game folder
   - Unity assemblies provided by MelonLoader after first game launch

## Why MelonLoader?

### Accessibility Wins 🎯

- **Console output works with screen readers** - BepInEx logs are hard to read
- **Better error messages** - easier to debug without sight
- **Simpler workflow** - no need to manually dump IL2CPP assemblies
- **Active accessible community** - MelonLoader Discord has accessibility-focused modders

### Technical Benefits

- **Auto IL2CPP interop** - generates all needed assemblies on first run
- **Harmony included** - no separate package needed
- **Faster iteration** - hot-reload support for some changes
- **Better Unity integration** - smoother IL2CPP handling

## What Stayed the Same

✅ **SRAL integration** - unchanged, still using the same P/Invoke wrapper  
✅ **Harmony patches** - same patching approach  
✅ **Project structure** - same files, just different base class  
✅ **Your workflow** - build → deploy → test  

## Updated Workflow

### Before (BepInEx)
```powershell
.\scripts\setup-bepinex.ps1      # Install BepInEx
.\scripts\dump-il2cpp.ps1        # Dump assemblies (slow!)
.\scripts\update-references.ps1  # Configure project
.\scripts\deploy-mod.ps1         # Build & deploy
```

### After (MelonLoader)
```powershell
.\scripts\setup-melonloader.ps1  # Install MelonLoader
# Launch game once (MelonLoader auto-generates everything)
.\scripts\update-references.ps1  # Configure project
.\scripts\deploy-mod.ps1         # Build & deploy
```

**Simpler!** No manual IL2CPP dumping needed.

## Files Changed

- ✏️ `Plugin.cs` - converted to MelonMod
- ✏️ `CKFlashpointAccessibility.csproj` - updated references
- ✏️ `scripts/setup-bepinex.ps1` → renamed to `setup-melonloader.ps1`
- ✏️ `scripts/update-references.ps1` - updated for MelonLoader paths
- ✏️ `scripts/deploy-mod.ps1` - updated deployment target
- ✏️ All documentation files updated

## Next Steps

Everything is ready! Just:

1. Build SRAL.dll (see `docs/building-sral.md`)
2. Purchase the game
3. Run `.\scripts\setup-melonloader.ps1`
4. Launch game once
5. Build and deploy!

## Resources

- **MelonLoader Docs**: https://melonwiki.xyz
- **MelonLoader Discord**: https://discord.gg/2Wn3N2P
- **Accessibility Channel**: Ask about screen reader support!

---

**Note**: The project won't compile until you install the game and MelonLoader, since it needs `MelonLoader.dll`. This is normal and expected!
