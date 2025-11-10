# 🎮 Cyber Knights: Flashpoint Accessibility Mod - Setup Complete! ✅

## What I've Set Up For You

Your development environment is **ready to go** for creating accessibility mods! Here's what's configured:

### ✅ Completed

1. **Full C# Mod Project**
   - MelonLoader mod structure (more accessible!)
   - SRAL screen reader integration (P/Invoke wrapper)
   - Harmony patching framework configured
   - Ready to build after game installation

2. **Project Files Created**
   - `CKFlashpointAccessibility.csproj` - Main project file
   - `Plugin.cs` - Mod entry point with SRAL helper
   - `SRAL.cs` - Complete C# wrapper for SRAL library
   - `Patches/UIPatches.cs` - Template for Unity UI patches

3. **Automation Scripts**
   - `setup-bepinex.ps1` - Installs BepInEx to game folder
   - `dump-il2cpp.ps1` - Extracts game assemblies
   - `update-references.ps1` - Configures project references
   - `deploy-mod.ps1` - Builds and deploys mod

4. **Documentation**
   - `README.md` - Complete project overview
   - `DOWNLOAD-TOOLS.md` - All download links
   - `docs/building-sral.md` - SRAL build instructions

### ⏳ What You Need To Do

#### Before Buying the Game

**1. Build or Download SRAL.dll**

Since SRAL doesn't have pre-built releases, you have two options:

**Option A: Build from source** (see `docs/building-sral.md`):
```powershell
cd tools
git clone https://github.com/blindgoofball/SRAL.git
cd SRAL
cmake . -B build -A x64
cmake --build build --config Release
Copy-Item "build\Release\SRAL.dll" -Destination "..\sral\"
```

**Option B: Ask for pre-built binary**
- Post in SRAL GitHub Discussions
- Or use System.Speech as temporary fallback (I can help switch to this)

**2. That's It!**

MelonLoader will be auto-downloaded by the setup script - no manual tool downloads needed!

#### After Buying the Game

**3. Install Game & MelonLoader**
```powershell
# Run setup script (auto-downloads and installs MelonLoader)
.\scripts\setup-melonloader.ps1
```

**4. Launch Game Once**
- MelonLoader will automatically generate all needed assemblies
- This takes 1-2 minutes on first launch
- Check `MelonLoader\Latest.log` for progress

**5. Update Project References**
```powershell
.\scripts\update-references.ps1
```

**6. Build & Deploy**
```powershell
.\scripts\deploy-mod.ps1
```

**7. Test!**
- Launch game via Steam
- Screen reader should announce "Mod loaded successfully!"
- Check `MelonLoader\Latest.log` for mod loading
- **Much more accessible** - MelonLoader outputs to console!

---

## Project Structure

```
Flashpoint-access/
├── CKFlashpointAccessibility/     ← Your mod code
│   ├── Plugin.cs                  ← Main entry point (MelonMod) ✅
│   ├── SRAL.cs                    ← Screen reader wrapper ✅
│   ├── Patches/UIPatches.cs       ← Unity patches (template) ✅
│   └── *.csproj                   ← Project file ✅
├── scripts/                       ← Automation tools ✅
│   ├── setup-melonloader.ps1      ← Auto-downloads MelonLoader
│   ├── update-references.ps1      ← Configures project
│   └── deploy-mod.ps1             ← Builds & deploys
├── docs/                          ← Guides ✅
│   └── building-sral.md
├── tools/                         ← Downloaded tools
│   └── sral/                      ⏳ Need SRAL.dll here
└── README.md                      ← Project overview ✅
```

---

## Quick Command Reference

```powershell
# After game purchase - full workflow
.\scripts\setup-melonloader.ps1          # Install MelonLoader
# Launch game once (generates assemblies)
.\scripts\update-references.ps1          # Fix project refs
.\scripts\deploy-mod.ps1                 # Build & deploy

# Manual build & deploy
cd CKFlashpointAccessibility
dotnet build --configuration Release
# Then copy bin\Release\net6.0\*.dll to game's Mods\
```

---

## Why MelonLoader + SRAL?

**MelonLoader** vs BepInEx:
✅ **Screen reader accessible** - console output works with screen readers  
✅ **Simpler setup** - auto-generates IL2CPP assemblies  
✅ **Better error messages** - easier debugging  
✅ **Active community** - more accessible modding support  

**SRAL** (Screen Reader Abstraction Library):
✅ **Modern** - actively maintained  
✅ **Cross-platform** - Windows, macOS, Linux  
✅ **Multiple engines** - UIA, NVDA, JAWS, SAPI, Speech Dispatcher  
✅ **Braille support** - outputs to braille displays  
✅ **Feature-rich** - rate, volume, pitch control  

The C# wrapper I created (`SRAL.cs`) gives you full P/Invoke access to SRAL.

---

## GitHub Copilot Tips

You have Copilot set up! Use it for:

**In Copilot Chat (Ctrl+Alt+I):**
- "Write a Harmony patch for Unity UI Button clicks"
- "How do I traverse Unity UI hierarchy to find text?"
- "Explain this IL2CPP dumped code: [paste code]"

**Inline Copilot:**
- Start typing `// Patch to announce...` and let it autocomplete
- Type function signatures and let it fill in logic

**Best model:** Claude 3.5 Sonnet (switch in chat dropdown)

---

## Current Build Status

```
✅ Project structure complete (MelonMod)
✅ SRAL wrapper complete
✅ Setup scripts ready
⏳ Waiting for SRAL.dll build
⏳ Waiting for game purchase
⏳ Will compile after game installation (needs MelonLoader.dll)
```

---

## Next Steps (Priority Order)

1. **Build SRAL.dll** (see `docs/building-sral.md`)
2. **Download Il2CppDumper** from releases page
3. **Download BepInEx IL2CPP** from releases page
4. **Purchase game** from Steam
5. **Run setup scripts** in order
6. **Start modding!**

---

## Support & Resources

- **MelonLoader Docs**: https://melonwiki.xyz
- **MelonLoader Discord**: https://discord.gg/2Wn3N2P (accessible community!)
- **Harmony Docs**: https://harmony.pardeike.net
- **SRAL GitHub**: https://github.com/blindgoofball/SRAL
- **Unity Scripting**: https://docs.unity3d.com/ScriptReference/

---

## You're All Set! 🚀

Everything is ready except for the external tools. Once you:
1. Build/get SRAL.dll
2. Buy the game
3. Run the setup scripts

You'll be writing accessibility patches with full GitHub Copilot support!

**Questions?** Check the docs or ask - I've set up everything to make this as smooth as possible for a first-time modder.
