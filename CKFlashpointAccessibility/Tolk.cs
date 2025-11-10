using System;
using System.Runtime.InteropServices;

namespace CKFlashpointAccessibility
{
    /// <summary>
    /// P/Invoke wrapper for Tolk screen reader library
    /// Tolk is a more reliable alternative to SRAL with better screen reader support
    /// </summary>
    public static class Tolk
    {
        private const string DllName = "Tolk.dll";

        /// <summary>
        /// Initializes Tolk. Must be called before any other Tolk functions.
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern void Tolk_Load();

        /// <summary>
        /// Uninitializes Tolk. Call when done using Tolk.
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Tolk_Unload();

        /// <summary>
        /// Returns true if a screen reader is detected.
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Tolk_IsLoaded();

        /// <summary>
        /// Detects and returns the name of the active screen reader, or null if none is active.
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public static extern string Tolk_DetectScreenReader();

        /// <summary>
        /// Returns true if the current screen reader supports speech.
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Tolk_HasSpeech();

        /// <summary>
        /// Returns true if the current screen reader supports braille.
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Tolk_HasBraille();

        /// <summary>
        /// Outputs text through both speech and braille if available.
        /// </summary>
        /// <param name="text">The text to output</param>
        /// <param name="interrupt">Whether to interrupt currently speaking text</param>
        /// <returns>True if successful</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Tolk_Output([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        /// <summary>
        /// Speaks text through the screen reader.
        /// </summary>
        /// <param name="text">The text to speak</param>
        /// <param name="interrupt">Whether to interrupt currently speaking text</param>
        /// <returns>True if successful</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Tolk_Speak([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        /// <summary>
        /// Outputs text to braille display.
        /// </summary>
        /// <param name="text">The text to display in braille</param>
        /// <returns>True if successful</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Tolk_Braille([MarshalAs(UnmanagedType.LPWStr)] string text);

        /// <summary>
        /// Silences the screen reader's speech output.
        /// </summary>
        /// <returns>True if successful</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Tolk_Silence();

        /// <summary>
        /// Returns true if the screen reader is currently speaking.
        /// Note: Not all screen readers support this query.
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Tolk_IsSpeaking();

        /// <summary>
        /// Enables or disables SAPI support (Windows built-in TTS as fallback).
        /// </summary>
        /// <param name="trySAPI">True to enable SAPI, false to disable</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Tolk_TrySAPI([MarshalAs(UnmanagedType.I1)] bool trySAPI);

        /// <summary>
        /// Sets whether SAPI should be preferred over screen readers.
        /// </summary>
        /// <param name="preferSAPI">True to prefer SAPI, false to prefer screen readers</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Tolk_PreferSAPI([MarshalAs(UnmanagedType.I1)] bool preferSAPI);
    }
}
