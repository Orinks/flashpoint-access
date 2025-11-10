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
        private static MelonPreferences_Entry<bool> _aggregateLabels;
        private static MelonPreferences_Entry<bool> _announceTypedChar;
        private static MelonPreferences_Entry<bool> _debugTextExtraction;

        public static bool ModEnabled => _modEnabled?.Value ?? true;
        public static bool AnnounceMenuItems => _announceMenuItems?.Value ?? true;
        public static bool AnnounceButtons => _announceButtons?.Value ?? true;
        public static int SpeechDelay => _speechDelay?.Value ?? 100;
        public static bool InterruptSpeech => _interruptSpeech?.Value ?? true;
        public static bool AggregateLabels => _aggregateLabels?.Value ?? true;
        public static bool AnnounceTypedChar => _announceTypedChar?.Value ?? true;
        public static bool DebugTextExtraction => _debugTextExtraction?.Value ?? false;

        public override void OnInitializeMelon()
        {
            // Create configuration
            _configCategory = MelonPreferences.CreateCategory("CKAccessibility");
            _modEnabled = _configCategory.CreateEntry("Enabled", true, "Enable/disable the accessibility mod");
            _announceMenuItems = _configCategory.CreateEntry("AnnounceMenuItems", true, "Announce menu items when focused");
            _announceButtons = _configCategory.CreateEntry("AnnounceButtons", true, "Announce buttons when hovered/focused");
            _speechDelay = _configCategory.CreateEntry("SpeechDelay", 100, "Delay between speech announcements (ms)");
            _interruptSpeech = _configCategory.CreateEntry("InterruptPrevious", true, "Interrupt previous speech");
            _aggregateLabels = _configCategory.CreateEntry("AggregateLabels", true, "Aggregate text from multiple UI sources for complete labels");
            _announceTypedChar = _configCategory.CreateEntry("AnnounceTypedChar", true, "Announce individual characters when typing in text inputs");
            _debugTextExtraction = _configCategory.CreateEntry("DebugTextExtraction", false, "Log detailed text extraction info for debugging");

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

            LoggerInstance.Msg("CK Flashpoint Accessibility v1.0.0 loaded!");
            LoggerInstance.Msg("Waiting for game assemblies to load...");
            
            // Test announcement
            SRALHelper.Speak("Mod ready", true);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            // Apply patches when first scene loads (assemblies are ready)
            if (buildIndex == 0 || buildIndex == 1)
            {
                ApplyPatches();
            }
        }

        private static bool _patchesApplied = false;

        private void ApplyPatches()
        {
            if (_patchesApplied) return;

            try
            {
                LoggerInstance.Msg("Game assemblies loaded, applying patches...");
                
                var harmony = new HarmonyLib.Harmony("com.joshua.ckflashpoint.accessibility");
                
                // Apply UI patches using runtime type resolution
                Patches.UIPatches.ApplyPatches(harmony);
                
                _patchesApplied = true;
                LoggerInstance.Msg("All Harmony patches applied successfully.");
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Failed to apply Harmony patches: {ex.Message}");
                LoggerInstance.Error($"Stack trace: {ex.StackTrace}");
            }
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

            // Try engines in order of preference:
            // 1. NVDA (we'll use direct controller for speech, but SRAL for braille)
            // 2. JAWS (commercial screen reader)
            // 3. SAPI (Windows built-in TTS - good fallback)
            // 4. UIA (last resort, known issues)
            
            _logger?.Msg("Attempting SRAL initialization with ENGINE_NVDA...");
            if (!SRAL.SRAL_Initialize(SRAL.Engine.ENGINE_NVDA))
            {
                _logger?.Warning("ENGINE_NVDA initialization failed, trying ENGINE_JAWS...");
                
                if (!SRAL.SRAL_Initialize(SRAL.Engine.ENGINE_JAWS))
                {
                    _logger?.Warning("ENGINE_JAWS initialization failed, trying ENGINE_SAPI...");
                    
                    if (!SRAL.SRAL_Initialize(SRAL.Engine.ENGINE_SAPI))
                    {
                        _logger?.Warning("ENGINE_SAPI initialization failed, trying ENGINE_UIA...");
                        
                        // Last resort: UIA (avoid if possible)
                        if (!SRAL.SRAL_Initialize(SRAL.Engine.ENGINE_UIA))
                        {
                            _logger?.Error("All SRAL engines failed!");
                            throw new Exception("Failed to initialize any SRAL engine");
                        }
                        _logger?.Warning("Using ENGINE_UIA (last resort, may have issues)");
                    }
                    else
                    {
                        _logger?.Msg("Successfully initialized with ENGINE_SAPI");
                    }
                }
                else
                {
                    _logger?.Msg("Successfully initialized with ENGINE_JAWS");
                }
            }
            else
            {
                _logger?.Msg("Successfully initialized with ENGINE_NVDA (will use direct controller for speech)");
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
                _logger?.Msg($"[Speech] Speaking: \"{text}\" (interrupt={interrupt}, length={text.Length})");
                
                bool speechSuccess = false;
                
                // Try NVDA controller client directly first (bypasses SRAL's buggy implementation)
                if (NVDAController.IsNVDARunning())
                {
                    if (interrupt)
                    {
                        NVDAController.StopSpeech();
                    }
                    speechSuccess = NVDAController.Speak(text);
                    _logger?.Msg($"[NVDA Direct] Result: {speechSuccess}");
                }
                
                // Try native SAPI if NVDA not available (bypasses SRAL's buggy implementation)
                if (!speechSuccess && SAPIController.Initialize())
                {
                    speechSuccess = SAPIController.Speak(text, interrupt);
                    _logger?.Msg($"[SAPI Direct] Result: {speechSuccess}");
                }
                
                // Last resort: Use SRAL (has string truncation bug but better than nothing)
                if (!speechSuccess)
                {
                    _logger?.Warning("[Speech] Falling back to SRAL (may only read first character)");
                    SRAL.SRAL_Speak(text, interrupt);
                }
                
                // Always send to braille via SRAL (supports multiple braille displays)
                try
                {
                    SRAL.SRAL_Braille(text);
                }
                catch (Exception brailleEx)
                {
                    _logger?.Warning($"Braille output failed: {brailleEx.Message}");
                }
                
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
