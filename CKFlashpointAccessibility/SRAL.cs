using System;
using System.Runtime.InteropServices;

namespace CKFlashpointAccessibility
{
    /// <summary>
    /// C# P/Invoke wrapper for SRAL (Screen Reader Abstraction Library)
    /// Based on SRAL.h from https://github.com/blindgoofball/SRAL
    /// </summary>
    public static class SRAL
    {
        private const string DllName = "SRAL.dll";

        // Engine types
        public enum Engine : uint
        {
            ENGINE_SPEECH_DISPATCHER = 1,
            ENGINE_NVDA = 2,
            ENGINE_SAPI = 4,
            ENGINE_AV_SPEECH = 8,
            ENGINE_JAWS = 16,
            ENGINE_UIA = 32
        }

        // Engine features
        [Flags]
        public enum EngineFeatures : uint
        {
            SUPPORTS_SPEECH = 128,
            SUPPORTS_BRAILLE = 256,
            SUPPORTS_SPEECH_RATE = 512,
            SUPPORTS_VOLUME = 1024,
            SUPPORTS_PITCH = 2048,
            SUPPORTS_INFLECTION = 4096,
            SUPPORTS_PAUSING = 8192,
            SUPPORTS_RESUME = 16384
        }

        // Core functions
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_Initialize(Engine engine);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_Uninitialize();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_Speak([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_Braille([MarshalAs(UnmanagedType.LPWStr)] string text);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_Output([MarshalAs(UnmanagedType.LPWStr)] string text, [MarshalAs(UnmanagedType.I1)] bool interrupt);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_StopSpeech();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_PauseSpeech();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_ResumeSpeech();

        // Rate control
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong SRAL_GetRate();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_SetRate(ulong rate);

        // Volume control
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong SRAL_GetVolume();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_SetVolume(ulong volume);

        // Pitch control
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong SRAL_GetPitch();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_SetPitch(ulong pitch);

        // Inflection control
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong SRAL_GetInflection();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_SetInflection(ulong inflection);

        // Engine info
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Engine SRAL_GetCurrentEngine();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern EngineFeatures SRAL_GetEngineFeatures(uint reserved);

        // Keyboard hooks
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_RegisterKeyboardHooks();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_UnregisterKeyboardHooks();

        // Delay
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_Delay(uint time);

        // Speech status
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SRAL_IsSpeaking();
    }
}
