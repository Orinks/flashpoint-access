# Tolk Integration Success Notes

## Problem Summary
Initially attempted to integrate Tolk screen reader library to replace buggy SRAL implementation. The game crashed immediately when calling `Tolk_IsLoaded()` after `Tolk_Load()`.

## Root Cause
Incorrect P/Invoke marshaling attributes were causing native crashes:

### Issues Found:
1. **Boolean Return Values**: Using `[return: MarshalAs(UnmanagedType.I1)]` on boolean returns caused crashes
2. **String Return Values**: Automatic string marshaling via `[return: MarshalAs(UnmanagedType.LPWStr)]` on `Tolk_DetectScreenReader()` can cause crashes
3. **Boolean Parameters**: Explicit `[MarshalAs(UnmanagedType.I1)]` on bool parameters was unnecessary

## Solution
Analyzed the working [FF6ScreenReader mod](https://github.com/BlindGuyNW/FF6ScreenReader) which successfully uses Tolk. Key differences:

### Working P/Invoke Signatures:

```csharp
// Boolean returns - NO explicit marshaling
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern bool Tolk_IsLoaded();

// String returns - Manual IntPtr conversion
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, 
    CharSet = CharSet.Unicode, EntryPoint = "Tolk_DetectScreenReader")]
private static extern IntPtr Tolk_DetectScreenReader_Native();

public static string Tolk_DetectScreenReader()
{
    IntPtr ptr = Tolk_DetectScreenReader_Native();
    if (ptr == IntPtr.Zero)
        return null;
    return Marshal.PtrToStringUni(ptr);
}

// String parameters with bool - Simple types only
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, 
    CharSet = CharSet.Unicode)]
public static extern bool Tolk_Output(string text, bool interrupt);
```

### Changes Made:
1. Removed all `[return: MarshalAs(UnmanagedType.I1)]` from boolean functions
2. Changed `Tolk_DetectScreenReader()` to return `IntPtr` and manually convert with `Marshal.PtrToStringUni()`
3. Removed all `[MarshalAs(UnmanagedType.I1)]` from boolean parameters
4. Removed `[MarshalAs(UnmanagedType.LPWStr)]` from string parameters (CharSet handles it)

## Testing Results
After fixes, Tolk integration works perfectly:
- ✅ `Tolk_Load()` succeeds
- ✅ `Tolk_IsLoaded()` returns true without crashing
- ✅ Detects SAPI screen reader successfully
- ✅ Speech output works: "New Game", "Options", "Credits", "Quit to Desktop"
- ✅ All `Tolk_Output()` calls succeed

## Advantages of Tolk Over SRAL
1. **No String Truncation**: Tolk passes complete strings correctly (SRAL had first-character bug)
2. **Better Screen Reader Support**: Direct support for NVDA, JAWS, SAPI, Narrator
3. **Maintained Library**: Actively maintained by accessibility community
4. **Simpler API**: Cleaner interface than SRAL
5. **Thread-Safe Design**: Built-in locking prevents concurrent native call crashes

## Implementation Notes
- Keep native NVDA controller and SAPI COM implementations as fallbacks
- Tolk handles NVDA/JAWS/SAPI automatically, no need for engine-specific code
- `TrySAPI(true)` enables SAPI fallback when no screen reader detected
- Always use `Output()` instead of `Speak()` alone to support braille displays

## References
- Working FF6ScreenReader Tolk implementation: https://github.com/BlindGuyNW/FF6ScreenReader/blob/main/FFVI_Mod/Tolk.cs
- Tolk library: https://github.com/dkager/tolk
- MelonLoader P/Invoke documentation: https://melonwiki.xyz/#/modders/il2cpp

## Status
✅ **COMPLETE** - Tolk integration working and merged to main branch (commit 37f82ad)
