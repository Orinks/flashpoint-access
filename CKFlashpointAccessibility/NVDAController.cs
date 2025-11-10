using System;
using System.Runtime.InteropServices;

namespace CKFlashpointAccessibility
{
    /// <summary>
    /// Direct P/Invoke wrapper for NVDA Controller Client
    /// Bypasses SRAL to test if the issue is with SRAL or NVDA
    /// </summary>
    public static class NVDAController
    {
        private const string DllName = "nvdaControllerClient64.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int nvdaController_testIfRunning();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int nvdaController_speakText([MarshalAs(UnmanagedType.LPWStr)] string text);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int nvdaController_cancelSpeech();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int nvdaController_brailleMessage([MarshalAs(UnmanagedType.LPWStr)] string message);

        public static bool IsNVDARunning()
        {
            try
            {
                return nvdaController_testIfRunning() == 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool Speak(string text)
        {
            try
            {
                return nvdaController_speakText(text) == 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool StopSpeech()
        {
            try
            {
                return nvdaController_cancelSpeech() == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
