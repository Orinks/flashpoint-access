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

        public override void OnUpdate()
        {
            // Call character creation navigation update
            try
            {
                CharacterCreationNavigation.Update();
            }
            catch (Exception ex)
            {
                LoggerInstance.Error($"Error in CharacterCreationNavigation.Update: {ex.Message}");
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

            // Initialize Tolk (better than SRAL - no string truncation bug)
            try
            {
                _logger?.Msg("Initializing Tolk screen reader library...");
                
                try
                {
                    // Enable SAPI as fallback
                    _logger?.Msg("Enabling SAPI fallback...");
                    Tolk.Tolk_TrySAPI(true);
                    
                    _logger?.Msg("Setting SAPI preference to false...");
                    // Don't prefer SAPI - use screen readers first
                    Tolk.Tolk_PreferSAPI(false);
                }
                catch (Exception sapEx)
                {
                    _logger?.Warning($"SAPI configuration failed (non-fatal): {sapEx.Message}");
                }
                
                // Load Tolk
                _logger?.Msg("Calling Tolk_Load()...");
                Tolk.Tolk_Load();
                _logger?.Msg("Tolk_Load() returned");
                
                _logger?.Msg("Checking if Tolk is loaded...");
                if (Tolk.Tolk_IsLoaded())
                {
                    _logger?.Msg("Detecting screen reader...");
                    string screenReader = Tolk.Tolk_DetectScreenReader();
                    
                    _logger?.Msg("Checking capabilities...");
                    bool hasSpeech = Tolk.Tolk_HasSpeech();
                    bool hasBraille = Tolk.Tolk_HasBraille();
                    
                    _logger?.Msg($"Tolk initialized successfully!");
                    _logger?.Msg($"Detected screen reader: {screenReader ?? "None (using SAPI)"}");
                    _logger?.Msg($"Speech support: {hasSpeech}, Braille support: {hasBraille}");
                    
                    _isInitialized = true;
                }
                else
                {
                    _logger?.Error("Tolk_IsLoaded() returned false");
                    throw new Exception("Failed to initialize Tolk");
                }
            }
            catch (DllNotFoundException dllEx)
            {
                _logger?.Error($"Tolk DLL not found: {dllEx.Message}");
                _logger?.Error("Make sure Tolk.dll and its dependencies are in the Mods folder");
                throw;
            }
            catch (Exception ex)
            {
                _logger?.Error($"Tolk initialization failed: {ex.Message}");
                _logger?.Error($"Exception type: {ex.GetType().Name}");
                _logger?.Error($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public static void Speak(string text, bool interrupt = false)
        {
            if (!_isInitialized || string.IsNullOrWhiteSpace(text))
                return;

            try
            {
                _logger?.Msg($"[Tolk] Speaking: \"{text}\" (interrupt={interrupt}, length={text.Length})");
                
                // Use Tolk_Output which handles both speech and braille automatically
                bool success = Tolk.Tolk_Output(text, interrupt);
                _logger?.Msg($"[Tolk] Output result: {success}");
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
            Tolk.Tolk_Silence();
        }

        public static void Uninitialize()
        {
            if (!_isInitialized) return;
            
            _logger?.Msg("Uninitializing Tolk...");
            Tolk.Tolk_Unload();
            _isInitialized = false;
        }

        public static void Shutdown()
        {
            Uninitialize();
        }
    }
}
