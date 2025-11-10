using System;
using System.Runtime.InteropServices;

namespace CKFlashpointAccessibility
{
    /// <summary>
    /// Direct wrapper for Windows SAPI (Speech API)
    /// Bypasses SRAL to avoid string truncation bug
    /// </summary>
    public static class SAPIController
    {
        private static dynamic _voice;
        private static bool _initialized = false;

        public static bool Initialize()
        {
            if (_initialized) return true;

            try
            {
                // Create SAPI SpVoice COM object
                Type spVoiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
                if (spVoiceType == null) return false;

                _voice = Activator.CreateInstance(spVoiceType);
                _initialized = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Speak(string text, bool interrupt = false)
        {
            if (!_initialized || _voice == null) return false;

            try
            {
                // SVSFlagsAsync = 1, SVSFPurgeBeforeSpeak = 2
                int flags = 1; // Async
                if (interrupt)
                {
                    flags |= 2; // Purge before speak
                }

                _voice.Speak(text, flags);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool StopSpeech()
        {
            if (!_initialized || _voice == null) return false;

            try
            {
                // SVSFPurgeBeforeSpeak = 2
                _voice.Speak("", 2);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
