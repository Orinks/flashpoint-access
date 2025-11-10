# Cyber Knights: Flashpoint Accessibility Mod

A MelonLoader mod that adds screen reader accessibility to Cyber Knights: Flashpoint using SRAL (Screen Reader Abstraction Library) and Harmony runtime patching.

## Project Status
- **Mod Status**: ✅ Fully functional - announces UI elements via screen reader
- **Screen Reader Support**: NVDA, JAWS, SAPI, UIA, and braille displays
- **Framework**: MelonLoader 0.6+ with IL2CPP interop
- **Current Version**: 1.0.0

## Quick Start

### For Users
1. **Install the game** from [Steam](https://store.steampowered.com/app/1021210/Cyber_Knights_Flashpoint/)
2. **Download the latest release** from the [Releases](https://github.com/Orinks/flashpoint-access/releases) page
3. **Extract to game directory** (typically `C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint`)
4. **Launch the game** - screen reader will announce "Cyber Knights Flashpoint accessibility mod loaded successfully!"
5. **Navigate with Tab/Arrow keys** - UI elements will be spoken automatically

### For Developers
1. **Clone this repository**: `git clone https://github.com/Orinks/flashpoint-access.git`
2. **Run setup script**: `.\scripts\setup-melonloader.ps1`
3. **Launch game once** to generate IL2CPP assemblies
4. **Update references**: `.\scripts\update-references.ps1`
5. **Build SRAL** (optional): See `docs\building-sral.md`
6. **Deploy the mod**: `.\scripts\deploy-mod.ps1`

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

### Development Dependencies (Auto-Downloaded)
- **MelonLoader**: IL2CPP mod loader framework
- **SRAL**: Screen Reader Abstraction Library (build from source or use pre-built)

## Features

- **Menu Navigation**: All buttons and UI elements announce on focus
- **Keyboard Navigation**: Full Tab/Arrow key support with auto-focus
- **Multiple Screen Readers**: Works with NVDA, JAWS, Windows Narrator, and SAPI
- **Braille Display Support**: Via SRAL library
- **Configurable**: Settings in `UserData\MelonPreferences.cfg`
- **Rate Limiting**: Prevents announcement spam
- **Debug Logging**: Detailed logs for troubleshooting

## Development Workflow

### Setup
1. Install game from Steam
2. Run `scripts\setup-melonloader.ps1` to install MelonLoader
3. Launch game once to generate IL2CPP assemblies
4. Run `scripts\update-references.ps1` to configure project

### Development
1. Use IL2CPP dumps (`dumped/dump.cs`) to identify UI classes
2. Write Harmony patches in `Patches/UIPatches.cs`
3. Test with `.\scripts\deploy-mod.ps1`
4. Check `MelonLoader\Latest.log` for debugging

### Distribution
1. Build release configuration
2. Package mod DLL + SRAL dependencies
3. Create GitHub release with installation instructions

## Key Technologies

- **MelonLoader**: Unity IL2CPP mod loader (screen reader accessible!)
- **Harmony**: Runtime method patching
- **SRAL**: Screen Reader Abstraction Library (NVDA, JAWS, SAPI, etc.)
- **.NET 6**: Modern C# development

## How It Works

The mod uses runtime patching to intercept Unity UI events:

1. **MelonLoader** loads the mod on game startup
2. **Harmony patches** intercept UI events (`OnSelect`, `OnPointerClick`, etc.)
3. **Text extraction** uses reflection to read from IL2CPP game components
4. **SRAL** announces text via active screen reader (NVDA, JAWS, etc.)
5. **Rate limiting** prevents duplicate announcements

See [IMPLEMENTATION-NOTES.md](IMPLEMENTATION-NOTES.md) for technical details.

## Resources

### Official Links
- **Game**: https://store.steampowered.com/app/1021210/Cyber_Knights_Flashpoint/
- **MelonLoader**: https://github.com/LavaGang/MelonLoader
- **SRAL**: https://github.com/blindgoofball/SRAL
- **Harmony**: https://harmony.pardeike.net/

### Documentation
- [Implementation Notes](IMPLEMENTATION-NOTES.md) - Technical architecture
- [Testing Guide](TESTING-GUIDE.md) - How to test the mod
- [UI Classes to Patch](UI-CLASSES-TO-PATCH.md) - IL2CPP class reference

## Technical Highlights

- **Runtime Type Resolution**: No compile-time dependencies on game types
- **IL2CPP Interop**: Uses `AccessTools` and reflection for game assembly access
- **Multi-Screen-Reader Support**: SRAL handles NVDA, JAWS, SAPI, UIA, braille
- **Private Field Access**: Extracts text from custom `STETextBlock` components
- **Harmony Patching**: Intercepts `OnSelect`, `OnDeselect`, and other UI events
- **Rate Limiting**: 100ms delay between announcements to prevent spam
- **Automated Deployment**: PowerShell scripts for building and testing

## Configuration

Edit `UserData\MelonPreferences.cfg` in the game directory:

```ini
[CK_Flashpoint_Accessibility]
Enabled = true
AnnounceButtons = true
AnnounceMenuItems = true
SpeechDelay = 100
InterruptSpeech = true
DebugTextExtraction = false
```

## Troubleshooting

**Mod not loading?**
- Check `MelonLoader\Latest.log` for errors
- Ensure `SRAL.dll` is in the `Mods` folder
- Verify screen reader (NVDA/JAWS) is running

**No speech output?**
- SRAL initialization message should appear in log
- Try restarting your screen reader
- Check Windows speech settings

**Text not extracted correctly?**
- Enable debug logging: `DebugTextExtraction = true`
- Check log for component type information
- Report issue with log excerpts

## Contributing

This is an open-source accessibility project! Contributions are welcome:

- **Bug Reports**: Open an issue describing the problem
- **Feature Requests**: Suggest new accessibility features
- **Pull Requests**: Submit improvements to text extraction or UI coverage
- **Testing**: Help test with different screen readers (NVDA, JAWS, Narrator)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Third-Party Licenses

- **SRAL**: See `tools/SRAL/LICENSE` (Screen Reader Abstraction Library)
- **MelonLoader**: LGPLv3 (https://github.com/LavaGang/MelonLoader)
- **Harmony**: MIT (https://github.com/pardeike/Harmony)
- **Il2CppInterop**: LGPL (https://github.com/BepInEx/Il2CppInterop)

## Credits

Created for accessibility improvements to make Cyber Knights: Flashpoint playable with screen readers.

**Developer**: Joshua Orinks ([@Orinks](https://github.com/Orinks))
**Framework**: MelonLoader + SRAL
**AI Assistant**: GitHub Copilot (Claude Sonnet 4.5)

## Disclaimer

This is an independent accessibility mod and is not affiliated with or endorsed by Trese Brothers, the developers of Cyber Knights: Flashpoint.
