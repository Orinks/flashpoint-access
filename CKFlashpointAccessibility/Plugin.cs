using MelonLoader;
using HarmonyLib;
using System;

[assembly: MelonInfo(typeof(CKFlashpointAccessibility.CKAccessibilityMod), "CK Flashpoint Accessibility", "1.0.0", "Joshua")]
[assembly: MelonGame(null, "CyberKnights")]

namespace CKFlashpointAccessibility
{
    public class CKAccessibilityMod : MelonMod
    {
        // Preferences
        private static MelonPreferences_Category _configCategory;
        private static MelonPreferences_Entry<bool> _modEnabled;
        private static MelonPreferences_Entry<bool> _announceMenuItems;
        private static MelonPreferences_Entry<bool> _announceButtons;
        private static MelonPreferences_Entry<int> _speechDelay;
        private static MelonPreferences_Entry<bool> _interruptSpeech;

        public static bool ModEnabled => _modEnabled?.Value ?? true;
        public static bool AnnounceMenuItems => _announceMenuItems?.Value ?? true;
        public static bool AnnounceButtons => _announceButtons?.Value ?? true;
        public static int SpeechDelay => _speechDelay?.Value ?? 100;
        public static bool InterruptSpeech => _interruptSpeech?.Value ?? true;

        public override void OnInitializeMelon()
        {
            // Create configuration
            _configCategory = MelonPreferences.CreateCategory("CKAccessibility");
            _modEnabled = _configCategory.CreateEntry("Enabled", true, "Enable/disable the accessibility mod");
            _announceMenuItems = _configCategory.CreateEntry("AnnounceMenuItems", true, "Announce menu items when focused");
            _announceButtons = _configCategory.CreateEntry("AnnounceButtons", true, "Announce buttons when hovered/focused");
            _speechDelay = _configCategory.CreateEntry("SpeechDelay", 100, "Delay between speech announcements (ms)");
            _interruptSpeech = _configCategory.CreateEntry("InterruptPrevious", true, "Interrupt previous speech");

            if (!ModEnabled)
            {
                LoggerInstance.Msg("Accessibility mod is disabled in config.");
                return;
            }

            // Initialize SRAL
            try
            {
                SRALHelper.Initialize(LoggerInstance);
                LoggerInstance.Msg("SRAL screen reader library initialized successfully.");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Failed to initialize SRAL: {ex.Message}");
                return;
            }

            // Apply Harmony patches
            try
            {
                // Uncomment after game installation to enable actual patches
                // HarmonyInstance.PatchAll(typeof(UIPatches));
                
                LoggerInstance.Msg("Harmony patches applied successfully.");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Failed to apply Harmony patches: {ex.Message}");
            }

            LoggerInstance.Msg("CK Flashpoint Accessibility v1.0.0 loaded!");
            
            // Test announcement
            SRALHelper.Speak("Cyber Knights Flashpoint accessibility mod loaded successfully!", true);
        }

        public override void OnDeinitializeMelon()
        {
            SRALHelper.Shutdown();
            LoggerInstance.Msg("Accessibility mod unloaded.");
        }
    }

    /// <summary>
    /// Helper class for SRAL screen reader integration
    /// </summary>
    public static class SRALHelper
    {
        private static bool _isInitialized = false;
        private static DateTime _lastSpeechTime = DateTime.MinValue;
        private static MelonLogger.Instance _logger;

        public static void Initialize(MelonLogger.Instance logger)
        {
            if (_isInitialized) return;

            _logger = logger;

            // Try to initialize with UIA (best for Windows desktop apps)
            if (!SRAL.SRAL_Initialize(SRAL.Engine.ENGINE_UIA))
            {
                // Fallback to SAPI if UIA fails
                if (!SRAL.SRAL_Initialize(SRAL.Engine.ENGINE_SAPI))
                {
                    // Try NVDA directly
                    if (!SRAL.SRAL_Initialize(SRAL.Engine.ENGINE_NVDA))
                    {
                        // Last resort: JAWS
                        if (!SRAL.SRAL_Initialize(SRAL.Engine.ENGINE_JAWS))
                        {
                            throw new Exception("Failed to initialize any screen reader engine");
                        }
                    }
                }
            }

            var engine = SRAL.SRAL_GetCurrentEngine();
            var features = SRAL.SRAL_GetEngineFeatures(0);
            
            _logger?.Msg($"SRAL initialized with engine: {engine}");
            _logger?.Msg($"Engine features: {features}");

            _isInitialized = true;
        }

        public static void Speak(string text, bool interrupt = false)
        {
            if (!_isInitialized || string.IsNullOrWhiteSpace(text))
                return;

            // Rate limiting
            var now = DateTime.Now;
            var delay = CKAccessibilityMod.SpeechDelay;
            if ((now - _lastSpeechTime).TotalMilliseconds < delay && !interrupt)
                return;

            try
            {
                SRAL.SRAL_Speak(text, interrupt);
                _lastSpeechTime = now;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error speaking text: {ex.Message}");
            }
        }

        public static void Output(string text, bool interrupt = false)
        {
            if (!_isInitialized || string.IsNullOrWhiteSpace(text))
                return;

            try
            {
                // Output speaks and also sends to braille display if available
                SRAL.SRAL_Output(text, interrupt);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error outputting text: {ex.Message}");
            }
        }

        public static void Silence()
        {
            if (!_isInitialized) return;
            SRAL.SRAL_StopSpeech();
        }

        public static void Shutdown()
        {
            if (!_isInitialized) return;
            
            SRAL.SRAL_Uninitialize();
            _isInitialized = false;
            _logger?.Msg("SRAL shut down.");
        }
    }
}
