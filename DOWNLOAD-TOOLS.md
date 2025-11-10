# Required Tools and Downloads

## Before Buying the Game

### 1. Download SRAL (Screen Reader Abstraction Library)
**Link**: https://github.com/blindgoofball/SRAL

**What you need**:
- Clone or download the repository: `git clone https://github.com/blindgoofball/SRAL.git`
- Build using CMake (requires Visual Studio with C++ tools):
  ```
  cmake . -B build
  cmake --build build --config Release
  ```
- You'll get `SRAL.dll` in `build/Release/`

**Files needed from SRAL**:
- `SRAL.dll` (x64 version) - main library
- `nvdaControllerClient64.dll` (download separately from NVDA)

**Where to extract**: `Flashpoint-access\tools\sral\`

**Alternative**: I've created a C# wrapper in the project that uses P/Invoke, so you just need the compiled `SRAL.dll`

---

### 2. Download MelonLoader (Automated)
**Link**: https://github.com/LavaGang/MelonLoader/releases

**What you need**:
- The setup script will download this automatically!
- Or manually download `MelonLoader.x64.zip` (latest version)
- Extract directly into the game folder after purchasing

**Why MelonLoader**: More accessible with screen readers, simpler setup, auto-generates IL2CPP assemblies

---

### 3. Il2CppDumper (Optional)
**Link**: https://github.com/Perfare/Il2CppDumper/releases

**Status**: Not needed! MelonLoader automatically generates managed assemblies on first run

---

## After Buying the Game

### 4. Install BepInEx
1. Extract `BepInEx_UnityIL2CPP_x64_*.zip` directly into game folder:
   - Default: `C:\Program Files (x86)\Steam\steamapps\common\Cyber Knights Flashpoint\`
2. Launch game once to generate config files
3. Look for `BepInEx\LogOutput.log` to confirm it worked

### 5. Dump Game Assemblies
1. Run Il2CppDumper:
   ```
   Il2CppDumper.exe "GameAssembly.dll" "Cyber_Knights__Flashpoint_Data\il2cpp_data\Metadata\global-metadata.dat" ".\output"
   ```
2. Copy the `DummyDll` folder contents to your project's references

---

## Quick Reference

| Tool | Purpose | Download Link |
|------|---------|--------------||
| **SRAL** | Screen reader support | https://github.com/blindgoofball/SRAL |
| **MelonLoader** | Mod loader (auto-downloaded by script) | https://github.com/LavaGang/MelonLoader/releases |
| **dnSpy** (optional) | Decompiler to explore code | https://github.com/dnSpy/dnSpy/releases |

---

## Current Status

✅ Project created and configured  
✅ SRAL C# wrapper created (P/Invoke)  
✅ MelonLoader setup (script will auto-download)  
⏳ **YOU NEED TO BUILD**: SRAL.dll (see links above)  
⏳ Purchase game from Steam  
⏳ Run setup script  

---

## Notes

- The PowerShell scripts I created can automate steps 4-5, but you need to download the tools first
- Alternatively, just extract everything manually following the steps above
- All tools are free and open source
