# Cyber Knights: Flashpoint Accessibility Mod Project

This workspace is set up to create accessibility modifications for Cyber Knights: Flashpoint using BepInEx and Tolk screen reader integration.

## Project Status
- **Pre-Purchase Setup**: ✅ Complete development environment ready
- **Code Implementation**: ✅ 3-tier text extraction with IL2CPP interop
- **Debugging Tools**: ✅ Enhanced logging + UnityExplorer installer
- **Next Step**: 🎮 Test in-game and report findings (see TESTING-WORKFLOW.md)

## Quick Start (After Game Purchase)

1. **Install the game** from [Steam](https://store.steampowered.com/app/1021210/Cyber_Knights_Flashpoint/)
2. **Run the setup script**: `.\scripts\setup-melonloader.ps1`
3. **Launch game once** to generate MelonLoader files
4. **Update project references**: `.\scripts\update-references.ps1`
5. **Build SRAL**: See `docs\building-sral.md` (screen reader library)
6. **Deploy the mod**: `.\scripts\deploy-mod.ps1` (builds + copies to Mods/)
7. **Enable debug logging**: Edit `UserData\MelonPreferences.cfg`, set `DebugTextExtraction = true`
8. **Test and report**: Follow `TESTING-WORKFLOW.md`

## Project Structure

```
Flashpoint-access/
├── CKFlashpointAccessibility/     # Main mod project (C# class library)
├── tools/                         # Downloaded tools (Il2CppDumper, etc.)
├── dumped/                        # IL2CPP dumped assemblies (post-purchase)
├── scripts/                       # PowerShell automation scripts
├── docs/                          # Guides and documentation
└── README.md                      # This file
```

## Prerequisites (Install These Now)

### Required Software
- [x] **VS Code**: https://code.visualstudio.com/
- [x] **.NET 8.0 SDK**: https://dotnet.microsoft.com/download (LTS version)
- [ ] **Git** (optional): https://git-scm.com/download/win

### VS Code Extensions (Install via Ctrl+Shift+X)
- `ms-dotnettools.csharp` - C# by Microsoft
- `ms-dotnettools.csdevkit` - C# Dev Kit
- `github.copilot` - GitHub Copilot
- `github.copilot-chat` - GitHub Copilot Chat

### After Game Purchase
- **BepInEx IL2CPP x64**: Auto-downloaded by setup script
- **Il2CppDumper**: Auto-downloaded by setup script
- **Tolk**: Already included in `tools/tolk/`

## Development Workflow

### Phase 1: Setup (Before Game Purchase) ✅
- [x] Create project structure
- [x] Set up VS Code workspace
- [x] Create mod template with BepInEx plugin structure
- [x] Integrate Tolk library references
- [x] Create helper scripts

### Phase 2: Post-Purchase Setup
1. Install game via Steam
2. Run `scripts\setup-bepinex.ps1` to install BepInEx
3. Launch game once to generate BepInEx folders
4. Run `scripts\dump-il2cpp.ps1` to extract game assemblies
5. Run `scripts\update-references.ps1` to configure project

### Phase 3: Development
1. Use Il2CppDumper output to understand game structure
2. Write Harmony patches for UI components
3. Integrate Tolk for screen reader output
4. Test with NVDA/JAWS
5. Iterate and expand features

### Phase 4: Distribution
1. Build release version
2. Package with instructions
3. Share on Steam forums/Reddit

## Key Technologies

- **MelonLoader**: Unity IL2CPP mod loader (screen reader accessible!)
- **Harmony**: Runtime method patching
- **SRAL**: Screen Reader Abstraction Library (NVDA, JAWS, SAPI, etc.)
- **.NET 6**: Modern C# development

## Accessibility Features Roadmap

| Priority | Feature | Implementation |
|----------|---------|----------------|
| P0 | Mod load announcement | Direct Tolk call on Awake |
| P0 | Menu navigation speech | Patch `Selectable.OnSelect()` |
| P1 | Button/UI element reading | Patch `EventSystem` selection |
| P1 | Keyboard navigation hints | Hook Input system |
| P2 | Combat HUD narration | Game-specific class patches |
| P2 | Dynamic content updates | Periodic UI scanning |
| P3 | Configuration options | BepInEx config file |

## Resources

### Official Links
- **Game**: https://store.steampowered.com/app/1021210/Cyber_Knights_Flashpoint/
- **BepInEx**: https://github.com/BepInEx/BepInEx
- **Il2CppDumper**: https://github.com/Perfare/Il2CppDumper
- **Tolk**: https://github.com/dkager/tolk
- **HarmonyX**: https://github.com/BepInEx/HarmonyX

### Learning Materials
- BepInEx IL2CPP Plugin Guide: https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/
- Harmony Documentation: https://harmony.pardeike.net/
- Unity Scripting Reference: https://docs.unity3d.com/ScriptReference/

## Implementation Highlights

- **3-Tier Text Extraction**: Direct IL2CPP access → Reflection with 3 assembly names → Private field access
- **Enhanced Debug Logging**: Logs all component types, assemblies, and text properties for troubleshooting
- **MelonLoader 0.6 + CoreCLR**: Modern .NET 6 IL2CPP interop (not legacy Unhollower)
- **SRAL Integration**: Multi-screen-reader support (NVDA, JAWS, SAPI, UIA, braille displays)
- **Automated Tooling**: UnityExplorer installer, deployment scripts, reference updater

See `PLAN-IMPLEMENTATION-STATUS.md` for detailed comparison with troubleshooting plan.

## Notes

- **Game Architecture**: Unity IL2CPP (confirmed by Steam page tech specs)
- **Expected Files**: `GameAssembly.dll`, `global-metadata.dat`
- **Default Install Path**: `C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint`
- **This is pioneering work**: No existing accessibility mods found as of Nov 2024
- **Screen Reader Friendly Development**: MelonLoader console output works with NVDA/JAWS

## Troubleshooting

See `docs/troubleshooting.md` for common issues and solutions.

## Credits

Created for accessibility improvements to make Cyber Knights: Flashpoint playable with screen readers.

**Developer**: Joshua (2025)
**Framework**: BepInEx + Tolk
**AI Assistant**: GitHub Copilot (Claude 3.5 Sonnet)
