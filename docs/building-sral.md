# Building SRAL from Source

SRAL (Screen Reader Abstraction Library) doesn't have pre-built releases, so you need to compile it yourself.

## Prerequisites

1. **Visual Studio 2022** (Community Edition is free)
   - Download: https://visualstudio.microsoft.com/downloads/
   - During installation, select "Desktop development with C++"

2. **CMake** (if not included with VS)
   - Download: https://cmake.org/download/
   - Or install via: `winget install Kitware.CMake`

3. **Git** (to clone the repository)
   - Download: https://git-scm.com/download/win
   - Or install via: `winget install Git.Git`

## Build Steps

### Option 1: Using PowerShell (Recommended)

```powershell
# 1. Clone the repository (from the project root)
cd tools
git clone https://github.com/blindgoofball/SRAL.git
cd SRAL

# 2. Build with CMake
cmake . -B build -A x64
cmake --build build --config Release

# 3. Copy the DLL
New-Item -ItemType Directory -Path "..\sral" -Force
Copy-Item "build\Release\SRAL.dll" -Destination "..\sral\"

# 4. Download NVDA Controller Client (optional, for NVDA support)
# Download from: https://www.nvaccess.org/files/nvda/releases/stable/
# Extract nvdaControllerClient64.dll to tools\sral\
```

### Option 2: Using Visual Studio GUI

1. Open Visual Studio 2022
2. File → Open → CMake → Browse to `SRAL\CMakeLists.txt`
3. Build → Build All (or press Ctrl+Shift+B)
4. Find `SRAL.dll` in `out\build\x64-Release\`
5. Copy to `Flashpoint-access\tools\sral\`

## Verify Build

After building, you should have:

```
Flashpoint-access\tools\sral\
├── SRAL.dll                      (your build)
└── nvdaControllerClient64.dll    (optional, from NVDA website)
```

## Alternative: Use Pre-Built Binary

If you can't build from source, you can:

1. Ask in the SRAL GitHub discussions for a pre-built binary
2. Use an alternative like System.Speech (built into .NET)
3. Wait for me to provide a pre-built version after testing

## Troubleshooting

**CMake not found:**
- Make sure CMake is in your PATH
- Restart PowerShell after installation

**Build errors:**
- Ensure you have "Desktop development with C++" workload in Visual Studio
- Try using Visual Studio Developer PowerShell instead

**Missing dependencies:**
- SRAL should build without external dependencies on Windows
- Make sure you're targeting x64 architecture

## After Building

Once you have `SRAL.dll`, the C# mod project will automatically copy it to the output when you build the mod:

```powershell
cd CKFlashpointAccessibility
dotnet build
```

The DLL will be included in the mod package for deployment to the game.
