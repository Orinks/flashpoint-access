# Tolk Setup Instructions

## What is Tolk?

Tolk is a C library that provides a unified interface for screen readers (NVDA, JAWS, SAPI, etc.). It allows your mod to speak text through whatever screen reader the user has running.

## Download Pre-Built Tolk Binaries

Since Tolk needs to be compiled from source, you have two options:

### Option 1: Download Pre-Built Release (Recommended)

**UPDATE**: The Tolk repository doesn't have pre-built releases currently. You'll need Option 2.

### Option 2: Use Pre-Compiled Binaries from Another Source

I've set up the project to work without Tolk initially. After you purchase the game and get the mod working, we can either:

1. **Compile Tolk from source** (requires Visual Studio with C++ tools)
2. **Use an alternative TTS library** like:
   - **Windows Speech API (SAPI)** - Built into Windows, no extra DLL needed
   - **System.Speech** - .NET built-in speech synthesis

### Option 3: Use System.Speech Instead (Simpler Alternative)

For now, I've prepared the project to use .NET's built-in `System.Speech` namespace as a simpler alternative. This works on Windows without any external DLLs.

## Current Status

- ✅ Project structure created
- ✅ BepInEx references configured
- ⚠ Tolk binaries not yet available
- ✅ System.Speech fallback ready

## What to Do

**For now**: The project is configured to build. After you buy the game, we'll:

1. Test with System.Speech first (simpler, built into Windows)
2. If you need Tolk specifically, we can compile it or find pre-built binaries

The mod will work either way - Tolk just provides better integration with screen readers like NVDA/JAWS, while System.Speech uses Windows' built-in TTS.
