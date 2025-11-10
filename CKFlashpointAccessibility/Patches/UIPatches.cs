using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;

namespace CKFlashpointAccessibility.Patches
{
    /// <summary>
    /// Harmony patches for Cyber Knights: Flashpoint UI components
    /// Based on IL2CPP dump analysis
    /// 
    /// Primary Classes Patched:
    /// - STEButton: Main button interaction class (clicks, hover)
    /// - STETextBlock: Primary text display component
    /// - UIScreenBase: Base screen class for transitions
    /// - STESelectableWidgetBase: Selectable UI elements
    /// - STEDialogAnswerButton: Dialog choice buttons
    /// - STETextInput: Text input fields
    /// - STETextItem: List items
    /// 
    /// Screen-Specific Patches:
    /// - Screen_MissionPlanning, Screen_Roster, Screen_Loadout
    /// - Screen_Training, Screen_Cyberdeck
    /// 
    /// See UI-CLASSES-TO-PATCH.md for full analysis
    /// 
    /// NOTE: Uses runtime type resolution via AccessTools to avoid compile-time dependencies
    /// </summary>
    public static class UIPatches
    {
        // Text tracking for deduplication
        private static Dictionary<string, DateTime> _recentTextUpdates = new Dictionary<string, DateTime>();
        private static readonly TimeSpan TextDeduplicationWindow = TimeSpan.FromMilliseconds(500);
        
        // Character creation screen flood prevention
        private static bool _inCharacterCreation = false;
        private static DateTime _characterCreationEnterTime = DateTime.MinValue;
        private static readonly TimeSpan CharacterCreationInitDelay = TimeSpan.FromSeconds(2);
        
        /// <summary>
        /// Apply all UI patches using runtime type resolution
        /// </summary>
        public static void ApplyPatches(HarmonyLib.Harmony harmony)
        {
            try
            {
                // Get the Il2Cpp game assembly at runtime
                var gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Il2CppCoreRPG_v1");

                if (gameAssembly == null)
                {
                    MelonLoader.MelonLogger.Warning("Il2CppCoreRPG_v1 assembly not found - UI patches skipped");
                    return;
                }
                
                MelonLoader.MelonLogger.Msg($"Found game assembly with {gameAssembly.GetTypes().Length} types");

                // Patch STEButton - Focus on selection, not hover
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.Buttons.STEButton", "OnPointerClick", 
                    postfix: nameof(STEButton_OnPointerClick_Postfix));
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.Buttons.STEButton", "OnSelect",
                    postfix: nameof(STEButton_OnSelect_Postfix));
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.Buttons.STEButton", "OnDeselect",
                    postfix: nameof(STEButton_OnDeselect_Postfix));

                // Patch STETextBlock
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.Text.STETextBlock", "SetText",
                    postfix: nameof(STETextBlock_SetText_Postfix));

                // Patch UIScreenBase
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.UIScreenBase", "Show",
                    postfix: nameof(UIScreenBase_Show_Postfix));

                // Note: STEButton inherits from STESelectableWidgetBase, so patching STEButton is sufficient
                // Removed STESelectableWidgetBase patches to avoid duplicate announcements

                // Patch STEDialogAnswerButton - Selection not hover
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.DialogBlocks.STEDialogAnswerButton", "OnSelect",
                    postfix: nameof(STEDialogAnswerButton_OnSelect_Postfix));

                // Patch STETextInput
                PatchType(harmony, gameAssembly, "Il2CppSTETextInput", "OnSelect",
                    postfix: nameof(STETextInput_OnSelect_Postfix));
                PatchType(harmony, gameAssembly, "Il2CppSTETextInput", "OnValueChanged",
                    postfix: nameof(STETextInput_OnValueChanged_Postfix));

                // Patch STETextItem - Selection not hover
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.ContentBlocks.STETextItem", "OnSelect",
                    postfix: nameof(STETextItem_OnSelect_Postfix));
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.ContentBlocks.STETextItem", "Select",
                    postfix: nameof(STETextItem_Select_Postfix));

                // Patch specific screens
                PatchType(harmony, gameAssembly, "Il2CppScreen_MissionPlanning", "Show",
                    postfix: nameof(Screen_MissionPlanning_Show_Postfix));
                PatchType(harmony, gameAssembly, "Il2CppScreen_Roster", "Show",
                    postfix: nameof(Screen_Roster_Show_Postfix));
                PatchType(harmony, gameAssembly, "Il2CppScreen_Loadout", "Show",
                    postfix: nameof(Screen_Loadout_Show_Postfix));
                PatchType(harmony, gameAssembly, "Il2CppScreen_Training", "Show",
                    postfix: nameof(Screen_Training_Show_Postfix));
                PatchType(harmony, gameAssembly, "Il2CppScreen_Cyberdeck", "Show",
                    postfix: nameof(Screen_Cyberdeck_Show_Postfix));

                // Patch options menu controls - STESelectInput (dropdowns)
                // STESelectInput inherits from STEWidgetInputBase which implements OnSelect
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.Inputs.STESelectInput", "Select",
                    postfix: nameof(STESelectInput_Select_Postfix));
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.Inputs.STESelectInput", "SetIndex",
                    postfix: nameof(STESelectInput_SetIndex_Postfix));

                // Patch STEDefaultSliderWrapper - Used by sliders, inherits from Unity Slider
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.STEDefaultSliderWrapper", "OnSelect",
                    postfix: nameof(STEDefaultSliderWrapper_OnSelect_Postfix));

                // Patch STETabMenu for tab navigation announcements
                PatchType(harmony, gameAssembly, "Il2CppRPG.UI.Widgets.Tabs.STETabMenu", "SelectTab",
                    postfix: nameof(STETabMenu_SelectTab_Postfix));

                // Add catch-all Unity Selectable patch to see ALL selections
                try
                {
                    var selectableType = typeof(UnityEngine.UI.Selectable);
                    var onSelectMethod = AccessTools.Method(selectableType, "OnSelect");
                    if (onSelectMethod != null)
                    {
                        var postfixMethod = AccessTools.Method(typeof(UIPatches), nameof(Unity_Selectable_OnSelect_Postfix));
                        harmony.Patch(onSelectMethod, postfix: new HarmonyMethod(postfixMethod));
                        MelonLoader.MelonLogger.Msg("Patched: Unity Selectable.OnSelect (catch-all for debugging)");
                    }
                }
                catch (Exception ex)
                {
                    MelonLoader.MelonLogger.Warning($"Failed to patch Unity Selectable.OnSelect: {ex.Message}");
                }

                MelonLoader.MelonLogger.Msg("All UI patches applied successfully");
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error applying UI patches: {ex.Message}");
            }
        }

        private static void PatchType(HarmonyLib.Harmony harmony, Assembly assembly, string typeName, string methodName, 
            string prefix = null!, string postfix = null!)
        {
            try
            {
                var type = assembly.GetType(typeName);
                if (type == null)
                {
                    MelonLoader.MelonLogger.Warning($"Type not found: {typeName}");
                    return;
                }

                var method = AccessTools.Method(type, methodName);
                if (method == null)
                {
                    MelonLoader.MelonLogger.Warning($"Method not found: {typeName}.{methodName}");
                    return;
                }

                var prefixMethod = prefix != null ? AccessTools.Method(typeof(UIPatches), prefix) : null;
                var postfixMethod = postfix != null ? AccessTools.Method(typeof(UIPatches), postfix) : null;

                harmony.Patch(method, 
                    prefix: prefixMethod != null ? new HarmonyMethod(prefixMethod) : null,
                    postfix: postfixMethod != null ? new HarmonyMethod(postfixMethod) : null);

                MelonLoader.MelonLogger.Msg($"Patched: {typeName}.{methodName}");
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Warning($"Failed to patch {typeName}.{methodName}: {ex.Message}");
            }
        }

        // ========== PATCH METHODS ==========

        public static void STEButton_OnPointerClick_Postfix(object __instance, PointerEventData eventData)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceButtons) return;

                string text = GetButtonText(__instance);
                if (!string.IsNullOrEmpty(text))
                {
                    MelonLoader.MelonLogger.Msg($"[Accessibility] Button activated: {text}");
                    SRALHelper.Speak($"Button activated: {text}", true);
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STEButton_OnPointerClick: {ex.Message}");
            }
        }

        public static void STEButton_OnSelect_Postfix(object __instance, BaseEventData eventData)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceButtons) return;

                string text = GetButtonText(__instance);
                MelonLoader.MelonLogger.Msg($"[Accessibility] Button selected, extracted text: \"{text}\" (Length: {text?.Length ?? 0})");
                
                if (!string.IsNullOrEmpty(text))
                {
                    // Clean up text - remove trailing numbers like " 56"
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+\d+$", "").Trim();
                    
                    // Remove parenthetical tags like "(Corp Knowledge)"
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s*\([^)]+\)\s*", " ").Trim();
                    
                    // Remove trailing dashes from truncated text
                    text = text.TrimEnd('-', ' ');
                    
                    // Detect navigation away from character creation screens
                    if (text == "Back" || text == "Next" || text == "Prev")
                    {
                        CharacterCreationNavigation.ExitAttributeScreen();
                    }
                    
                    SRALHelper.Speak(text, true);
                }
                else
                {
                    MelonLoader.MelonLogger.Warning("[Accessibility] Button text was null or empty!");
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STEButton_OnSelect: {ex.Message}");
                MelonLoader.MelonLogger.Error($"Stack trace: {ex.StackTrace}");
            }
        }

        public static void STEButton_OnDeselect_Postfix(object __instance, BaseEventData eventData)
        {
            try
            {
                // Optionally announce deselection or stay silent
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STEButton_OnDeselect: {ex.Message}");
            }
        }

        public static void STETextBlock_SetText_Postfix(object __instance, string t)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                if (!string.IsNullOrEmpty(t))
                {
                    // Filter out UI noise - only announce substantial text (story, dialog, etc.)
                    // Skip: single characters, numbers only, very short text, common UI labels
                    if (t.Length < 3) return;
                    if (System.Text.RegularExpressions.Regex.IsMatch(t, @"^\d+$")) return; // Skip pure numbers
                    if (System.Text.RegularExpressions.Regex.IsMatch(t, @"^[\d\.,\-%]+$")) return; // Skip numbers with symbols
                    
                    // Skip common UI labels and categories
                    string[] skipLabels = { "HP", "AP", "XP", "SP", "Level", "Cost", "Max", "Min", "Total", "OK", "Cancel",
                        "Body", "Suit", "Face", "Hair", "Eyes", "Eyebrows", "Cybernetics", "Tattoo", "Piercings",
                        "Hat", "Mask", "Helmet", "Backpack", "Eye Accessory", "Face Details" };
                    if (skipLabels.Any(label => t.Equals(label, StringComparison.OrdinalIgnoreCase))) return;
                    
                    // Get the GameObject name for context
                    var monoBehaviour = __instance as MonoBehaviour;
                    string objectName = monoBehaviour != null ? monoBehaviour.gameObject.name : "unknown";
                    
                    // Deduplication: Skip if same text was announced recently
                    var now = DateTime.Now;
                    if (_recentTextUpdates.TryGetValue(t, out DateTime lastTime))
                    {
                        if ((now - lastTime) < TextDeduplicationWindow)
                        {
                            MelonLoader.MelonLogger.Msg($"[Accessibility] STETextBlock.SetText - SKIPPED (duplicate): \"{t.Substring(0, Math.Min(50, t.Length))}...\"");
                            return;
                        }
                    }
                    _recentTextUpdates[t] = now;
                    
                    // Clean up old entries from dictionary (keep only last 100)
                    if (_recentTextUpdates.Count > 100)
                    {
                        var oldEntries = _recentTextUpdates.Where(kvp => (now - kvp.Value) > TimeSpan.FromSeconds(10)).ToList();
                        foreach (var entry in oldEntries)
                        {
                            _recentTextUpdates.Remove(entry.Key);
                        }
                    }
                    
                    // Log for debugging
                    MelonLoader.MelonLogger.Msg($"[Accessibility] STETextBlock.SetText - Object: \"{objectName}\", Text: \"{t.Substring(0, Math.Min(50, t.Length))}...\"");
                    
                    // Detect character creation screen entering
                    if (t.Contains("Create your Knight") || (t.Contains("Assign Attributes") && !_inCharacterCreation))
                    {
                        MelonLoader.MelonLogger.Msg("[Accessibility] Entering character creation screen");
                        _inCharacterCreation = true;
                        _characterCreationEnterTime = DateTime.Now;
                        
                        // Announce just the screen title
                        if (t.Contains("Create your Knight"))
                        {
                            SRALHelper.Speak("Create your Knight screen. Use Tab to navigate between sections, arrow keys within sections.", false);
                            return; // Don't announce again below
                        }
                    }
                    
                    // Suppress text flood during character creation screen initialization (first 2 seconds)
                    if (_inCharacterCreation && (DateTime.Now - _characterCreationEnterTime) < CharacterCreationInitDelay)
                    {
                        // Only announce these specific items during init
                        if (t.Contains("Wasteland Coyote") || t.Contains("Corp Employee") || 
                            t.Contains("Smuggler") || t.Contains("description that describes"))
                        {
                            // Allow backstory selection to be announced
                        }
                        else
                        {
                            MelonLoader.MelonLogger.Msg($"[Accessibility] SUPPRESSED during char creation init: \"{t.Substring(0, Math.Min(30, t.Length))}...\"");
                            return;
                        }
                    }
                    
                    // Detect attributes screen
                    if (t.Contains("Assign Attributes") || t.Contains("Assign your free Attribute points"))
                    {
                        MelonLoader.MelonLogger.Msg("[Accessibility] Detected Attributes screen");
                        MelonLoader.MelonCoroutines.Start(EnterAttributeScreenDelayed());
                    }
                    
                    // Calculate delay based on text length to allow typing animation to complete
                    // Short dialog text (< 100 chars) gets no delay
                    // Long story text gets delay to let animation complete
                    int delayMs = 0;
                    if (t.Length > 100 && !t.Contains("Abandon") && !objectName.Contains("Title") && !objectName.Contains("Desc"))
                    {
                        delayMs = Math.Min(3000, t.Length * 50); // Max 3 seconds, 50ms per character
                    }
                    
                    if (delayMs > 0)
                    {
                        // Use MelonCoroutines for delayed announcement
                        MelonLoader.MelonCoroutines.Start(DelayedAnnounce(t, delayMs));
                    }
                    else
                    {
                        // Announce immediately for short text/dialogs
                        SRALHelper.Speak(t, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STETextBlock_SetText: {ex.Message}");
            }
        }

        public static void UIScreenBase_Show_Postfix(object __instance)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                string screenName = __instance.GetType().Name.Replace("Screen_", "").Replace("_", " ");
                SRALHelper.Speak($"Entering {screenName} screen", true);

                // Try to auto-select first selectable UI element for keyboard navigation
                var monoBehaviour = __instance as MonoBehaviour;
                if (monoBehaviour != null)
                {
                    // Find EventSystem and set first selectable
                    var eventSystem = UnityEngine.EventSystems.EventSystem.current;
                    if (eventSystem != null)
                    {
                        // Find first selectable in the screen
                        var selectables = monoBehaviour.GetComponentsInChildren<UnityEngine.UI.Selectable>();
                        if (selectables != null && selectables.Length > 0)
                        {
                            var firstButton = selectables.FirstOrDefault(s => s.IsInteractable());
                            if (firstButton != null)
                            {
                                eventSystem.SetSelectedGameObject(firstButton.gameObject);
                                MelonLoader.MelonLogger.Msg($"Auto-selected first button: {firstButton.gameObject.name}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in UIScreenBase_Show: {ex.Message}");
            }
        }
        public static void STESelectableWidgetBase_Select_Postfix(object __instance)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                string text = GetWidgetText(__instance);
                if (!string.IsNullOrEmpty(text))
                {
                    MelonLoader.MelonLogger.Msg($"[Accessibility] Widget selected: {text}");
                    SRALHelper.Speak(text, true);
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STESelectableWidgetBase_Select: {ex.Message}");
            }
        }

        public static void STESelectableWidgetBase_OnSelect_Postfix(object __instance, BaseEventData eventData)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                string text = GetWidgetText(__instance);
                if (!string.IsNullOrEmpty(text))
                {
                    SRALHelper.Speak(text, true);
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STESelectableWidgetBase_OnSelect: {ex.Message}");
            }
        }

        public static void STESelectableWidgetBase_Deselect_Postfix(object __instance)
        {
            try
            {
                // Deselection - stay silent or optionally announce
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STESelectableWidgetBase_Deselect: {ex.Message}");
            }
        }

        public static void STEDialogAnswerButton_OnSelect_Postfix(object __instance, BaseEventData eventData)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceButtons) return;

                string text = GetButtonText(__instance);
                if (!string.IsNullOrEmpty(text))
                {
                    // Clean up text - remove trailing numbers
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+\d+$", "").Trim();
                    SRALHelper.Speak(text, true);
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STEDialogAnswerButton_OnSelect: {ex.Message}");
            }
        }

        public static void STETextInput_OnSelect_Postfix(object __instance, BaseEventData eventData)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;
                SRALHelper.Speak("Text input field", true);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STETextInput_OnSelect: {ex.Message}");
            }
        }

        public static void STETextInput_OnValueChanged_Postfix(object __instance, string value)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems || !CKAccessibilityMod.AnnounceTypedChar) return;

                if (!string.IsNullOrEmpty(value) && value.Length > 0)
                {
                    char lastChar = value[value.Length - 1];
                    SRALHelper.Speak(lastChar.ToString(), true);
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STETextInput_OnValueChanged: {ex.Message}");
            }
        }

        public static void STETextItem_OnSelect_Postfix(object __instance, BaseEventData eventData)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                string text = GetWidgetText(__instance);
                if (!string.IsNullOrEmpty(text))
                {
                    SRALHelper.Speak(text, true);
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STETextItem_OnSelect: {ex.Message}");
            }
        }

        public static void STETextItem_Select_Postfix(object __instance)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                string text = GetWidgetText(__instance);
                if (!string.IsNullOrEmpty(text))
                {
                    SRALHelper.Speak(text, true);
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STETextItem_Select: {ex.Message}");
            }
        }

        public static void Screen_MissionPlanning_Show_Postfix(object __instance)
        {
            try
            {
                SRALHelper.Speak("Mission Planning screen", true);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in Screen_MissionPlanning_Show: {ex.Message}");
            }
        }

        public static void Screen_Roster_Show_Postfix(object __instance)
        {
            try
            {
                SRALHelper.Speak("Roster screen", true);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in Screen_Roster_Show: {ex.Message}");
            }
        }

        public static void Screen_Loadout_Show_Postfix(object __instance)
        {
            try
            {
                SRALHelper.Speak("Loadout screen", true);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in Screen_Loadout_Show: {ex.Message}");
            }
        }

        public static void Screen_Training_Show_Postfix(object __instance)
        {
            try
            {
                SRALHelper.Speak("Training screen", true);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in Screen_Training_Show: {ex.Message}");
            }
        }

        public static void Screen_Cyberdeck_Show_Postfix(object __instance)
        {
            try
            {
                SRALHelper.Speak("Cyberdeck screen", true);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in Screen_Cyberdeck_Show: {ex.Message}");
            }
        }

        // ========== OPTIONS MENU CONTROLS ==========

        public static void STESelectInput_Select_Postfix(object __instance)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                var monoBehaviour = __instance as MonoBehaviour;
                if (monoBehaviour == null) return;

                // Get the label and current value
                var labelField = __instance.GetType().GetField("label", BindingFlags.NonPublic | BindingFlags.Instance);
                var dropdownField = __instance.GetType().GetField("m_Dropdown", BindingFlags.NonPublic | BindingFlags.Instance);
                
                string labelText = "";
                string currentValue = "";

                if (labelField != null)
                {
                    var labelObj = labelField.GetValue(__instance);
                    if (labelObj != null)
                    {
                        var textProp = labelObj.GetType().GetProperty("text");
                        if (textProp != null)
                        {
                            labelText = textProp.GetValue(labelObj)?.ToString() ?? "";
                        }
                    }
                }

                if (dropdownField != null)
                {
                    var dropdownObj = dropdownField.GetValue(__instance);
                    if (dropdownObj != null)
                    {
                        // Try to get value property
                        var valueProp = dropdownObj.GetType().GetProperty("value");
                        if (valueProp != null)
                        {
                            var valueIndex = valueProp.GetValue(dropdownObj);
                            
                            // Get options list
                            var optionsProp = dropdownObj.GetType().GetProperty("options");
                            if (optionsProp != null)
                            {
                                var options = optionsProp.GetValue(dropdownObj);
                                if (options != null && valueIndex != null)
                                {
                                    // Access the list and get the item at index
                                    var listType = options.GetType();
                                    var indexer = listType.GetProperty("Item");
                                    if (indexer != null)
                                    {
                                        var option = indexer.GetValue(options, new object[] { valueIndex });
                                        if (option != null)
                                        {
                                            var textProp = option.GetType().GetProperty("text");
                                            if (textProp != null)
                                            {
                                                currentValue = textProp.GetValue(option)?.ToString() ?? "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                string announcement = string.IsNullOrEmpty(labelText) 
                    ? $"Dropdown, {currentValue}"
                    : $"{labelText}, {currentValue}";
                
                MelonLoader.MelonLogger.Msg($"[Accessibility] Dropdown selected - Label: \"{labelText}\", Value: \"{currentValue}\", Full: \"{announcement}\"");
                SRALHelper.Speak(announcement, true);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STESelectInput_Select: {ex.Message}");
            }
        }

        public static void STESelectInput_SetIndex_Postfix(object __instance, int index)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                // Get the text for the new index
                var dropdownField = __instance.GetType().GetField("m_Dropdown", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (dropdownField != null)
                {
                    var dropdownObj = dropdownField.GetValue(__instance);
                    if (dropdownObj != null)
                    {
                        var optionsProp = dropdownObj.GetType().GetProperty("options");
                        if (optionsProp != null)
                        {
                            var options = optionsProp.GetValue(dropdownObj);
                            if (options != null)
                            {
                                var listType = options.GetType();
                                var indexer = listType.GetProperty("Item");
                                if (indexer != null)
                                {
                                    var option = indexer.GetValue(options, new object[] { index });
                                    if (option != null)
                                    {
                                        var textProp = option.GetType().GetProperty("text");
                                        if (textProp != null)
                                        {
                                            var text = textProp.GetValue(option)?.ToString() ?? "";
                                            if (!string.IsNullOrEmpty(text))
                                            {
                                                SRALHelper.Speak(text, true);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STESelectInput_SetIndex: {ex.Message}");
            }
        }

        public static void STEDefaultSliderWrapper_OnSelect_Postfix(object __instance, BaseEventData eventData)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                var slider = __instance as Slider;
                if (slider == null) return;

                float value = slider.value;
                float min = slider.minValue;
                float max = slider.maxValue;
                
                // Try to find parent STEInputSlider or other slider component with label
                string label = "";
                var transform = slider.transform;
                while (transform != null && string.IsNullOrEmpty(label))
                {
                    // Check for STEInputSlider parent
                    var inputSlider = transform.GetComponent<MonoBehaviour>();
                    if (inputSlider != null && inputSlider.GetType().Name == "STEInputSlider")
                    {
                        // Try to get nameTextBlock field
                        var nameField = inputSlider.GetType().GetField("nameTextBlock", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (nameField != null)
                        {
                            var textBlock = nameField.GetValue(inputSlider);
                            if (textBlock != null)
                            {
                                var textProp = textBlock.GetType().GetProperty("text");
                                if (textProp != null)
                                {
                                    label = textProp.GetValue(textBlock)?.ToString() ?? "";
                                }
                            }
                        }
                    }
                    transform = transform.parent;
                }

                // Fallback to GameObject name
                if (string.IsNullOrEmpty(label))
                {
                    label = slider.gameObject.name
                        .Replace("Slider", "")
                        .Replace("UI/", "")
                        .Trim();
                }
                
                // Calculate percentage
                float percentage = max != min ? ((value - min) / (max - min)) * 100f : 0f;
                
                // Try harder to find the actual label from parent STEInputSlider
                if (string.IsNullOrEmpty(label) || label.Contains("#") || label.Contains("Safehouse"))
                {
                    var parentTransform = slider.transform;
                    for (int i = 0; i < 5 && parentTransform != null; i++)
                    {
                        var components = parentTransform.GetComponents<MonoBehaviour>();
                        foreach (var comp in components)
                        {
                            if (comp != null && comp.GetType().Name == "STEInputSlider")
                            {
                                // Try to get the name text block
                                var nameField = comp.GetType().GetField("nameTextBlock", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (nameField != null)
                                {
                                    var textBlock = nameField.GetValue(comp);
                                    if (textBlock != null)
                                    {
                                        var textProp = textBlock.GetType().GetProperty("text");
                                        if (textProp != null)
                                        {
                                            var labelText = textProp.GetValue(textBlock)?.ToString();
                                            if (!string.IsNullOrWhiteSpace(labelText))
                                            {
                                                label = labelText;
                                                goto foundLabel;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        parentTransform = parentTransform.parent;
                    }
                    foundLabel:;
                }
                
                // Clean up label - remove GameObject prefixes if still needed
                if (!string.IsNullOrEmpty(label))
                {
                    label = label.Replace(" - #", "")
                                 .Replace("#", "")
                                 .Replace("Safehouse2/", "")
                                 .Replace("/Input", "")
                                 .Replace("Slider", "")
                                 .Trim();
                    
                    // Remove trailing numbers from GameObject names
                    label = System.Text.RegularExpressions.Regex.Replace(label, @"\s+\d+$", "").Trim();
                }
                
                // Debug: Show slider hierarchy for troubleshooting
                MelonLoader.MelonLogger.Msg($"[Accessibility] Slider debug - GameObject: \"{slider.gameObject.name}\"");
                var debugParent = slider.transform.parent;
                if (debugParent != null)
                {
                    MelonLoader.MelonLogger.Msg($"[Accessibility] Slider debug - Parent: \"{debugParent.name}\"");
                    
                    // Check for sibling text components (label might be next to slider, not parent)
                    for (int i = 0; i < debugParent.childCount; i++)
                    {
                        var child = debugParent.GetChild(i);
                        var textBlock = child.GetComponent<MonoBehaviour>();
                        if (textBlock != null && textBlock.GetType().Name == "STETextBlock")
                        {
                            var textProp = textBlock.GetType().GetProperty("text");
                            if (textProp != null)
                            {
                                var textValue = textProp.GetValue(textBlock)?.ToString();
                                if (!string.IsNullOrWhiteSpace(textValue))
                                {
                                    MelonLoader.MelonLogger.Msg($"[Accessibility] Slider debug - Found sibling STETextBlock on '{child.name}': \"{textValue}\"");
                                    if (string.IsNullOrEmpty(label) || label.Contains("#") || label.Contains("Input"))
                                    {
                                        label = textValue;
                                    }
                                }
                            }
                        }
                    }
                }
                
                string announcement = string.IsNullOrEmpty(label)
                    ? $"Slider, {percentage:F0} percent"
                    : $"{label}, {percentage:F0} percent";
                
                MelonLoader.MelonLogger.Msg($"[Accessibility] Slider final - Label: \"{label}\", Announcement: \"{announcement}\"");
                SRALHelper.Speak(announcement, true);
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STEDefaultSliderWrapper_OnSelect: {ex.Message}");
            }
        }

        public static void STETabMenu_SelectTab_Postfix(object __instance, int tabIndex)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                // Get the tab list
                var tabListField = __instance.GetType().GetField("tabList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (tabListField != null)
                {
                    var tabList = tabListField.GetValue(__instance);
                    if (tabList != null)
                    {
                        // Access the tab button at the index
                        var listType = tabList.GetType();
                        var itemProp = listType.GetProperty("Item");
                        if (itemProp != null && tabIndex >= 0)
                        {
                            try
                            {
                                var tabButton = itemProp.GetValue(tabList, new object[] { tabIndex });
                                if (tabButton != null)
                                {
                                    string tabText = GetButtonText(tabButton);
                                    if (!string.IsNullOrEmpty(tabText))
                                    {
                                        MelonLoader.MelonLogger.Msg($"[Accessibility] Tab changed to: \"{tabText}\" (index {tabIndex})");
                                        SRALHelper.Speak($"{tabText} tab", true);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MelonLoader.MelonLogger.Warning($"[Accessibility] Could not get tab at index {tabIndex}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in STETabMenu_SelectTab: {ex.Message}");
            }
        }

        // ========== HELPER METHODS ==========

        private static string GetButtonText(object button)
        {
            if (button == null)
            {
                MelonLoader.MelonLogger.Warning("[GetButtonText] Button is null");
                return string.Empty;
            }

            try
            {
                var buttonType = button.GetType();
                var candidates = new List<string>();
                var monoBehaviour = button as MonoBehaviour;

                if (CKAccessibilityMod.DebugTextExtraction)
                    MelonLoader.MelonLogger.Msg($"[GetButtonText] Button type: {buttonType.FullName}, GO: {monoBehaviour?.gameObject.name ?? "null"}");

                // STRATEGY 1: Direct IL2CPP component access (fastest, most reliable)
                // NOTE: Commented out because TMPro types may not be directly accessible in this game
                // Will rely on reflection-based strategies instead
                /*
                if (monoBehaviour != null)
                {
                    try
                    {
                        // Try direct GetComponent using TMPro types from Unity.TextMeshPro assembly
                        // MelonLoader exposes these as TMPro.* types after IL2CPP interop generation
                        var tmpComponent = monoBehaviour.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
                        if (tmpComponent != null && !string.IsNullOrWhiteSpace(tmpComponent.text))
                        {
                            if (CKAccessibilityMod.DebugTextExtraction)
                                MelonLoader.MelonLogger.Msg($"[GetButtonText] Direct TMP access: \"{tmpComponent.text}\"");
                            return tmpComponent.text;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (CKAccessibilityMod.DebugTextExtraction)
                            MelonLoader.MelonLogger.Warning($"[GetButtonText] Direct TMP access failed: {ex.Message}");
                    }
                }
                */

                // STRATEGY 2: Reflection-based search for TextMeshProUGUI components
                if (monoBehaviour != null)
                {
                    // Get all child GameObjects and check each for TextMeshProUGUI component
                    Transform[] allTransforms = monoBehaviour.GetComponentsInChildren<Transform>(true);
                    if (CKAccessibilityMod.DebugTextExtraction)
                        MelonLoader.MelonLogger.Msg($"[GetButtonText] Found {allTransforms.Length} child transforms");
                    
                    // DETAILED INSPECTION: Log ALL child transforms and their components
                    if (CKAccessibilityMod.DebugTextExtraction)
                    {
                        MelonLoader.MelonLogger.Msg($"[GetButtonText] Inspecting ALL {allTransforms.Length} child transforms:");
                        foreach (var trans in allTransforms)
                        {
                            Component[] comps = trans.GetComponents<Component>();
                            MelonLoader.MelonLogger.Msg($"  GameObject: {trans.gameObject.name} ({comps.Length} components):");
                            foreach (var c in comps)
                            {
                                if (c != null)
                                {
                                    // Use GetIl2CppType() for actual IL2CPP type name
                                    var il2cppType = c.GetIl2CppType();
                                    string typeName = il2cppType.FullName;
                                    MelonLoader.MelonLogger.Msg($"    - {typeName}");
                                }
                            }
                        }
                    }
                    
                    // Only inspect first child for detailed logging if no detailed inspection
                    if (CKAccessibilityMod.DebugTextExtraction && allTransforms.Length > 1 && false)
                    {
                        Component[] firstComps = allTransforms[1].GetComponents<Component>();
                        MelonLoader.MelonLogger.Msg($"[GetButtonText] Components on {allTransforms[1].gameObject.name}:");
                        foreach (var c in firstComps)
                        {
                            if (c != null)
                            {
                                // Get managed type info
                                var managedType = c.GetType();
                                string typeName = managedType.FullName;
                                string assemblyName = managedType.Assembly.GetName().Name;
                                
                                // Try getting the IL2CPP type if available
                                try
                                {
                                    var il2cppTypeMethod = managedType.GetProperty("Il2CppType", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (il2cppTypeMethod != null)
                                    {
                                        var il2cppType = il2cppTypeMethod.GetValue(c);
                                        typeName += $" (IL2CPP: {il2cppType})";
                                    }
                                }
                                catch { }
                                
                                MelonLoader.MelonLogger.Msg($"  - {typeName}");
                                MelonLoader.MelonLogger.Msg($"    Assembly: {assemblyName} | ToString: {c}");
                                
                                // Probe for text property
                                var textProp = managedType.GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
                                if (textProp != null)
                                {
                                    try
                                    {
                                        var textValue = textProp.GetValue(c);
                                        MelonLoader.MelonLogger.Msg($"    text property: \"{textValue}\"");
                                    }
                                    catch (Exception ex)
                                    {
                                        MelonLoader.MelonLogger.Msg($"    text property: ERROR - {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                    
                    // STRATEGY 2: Find TextMeshProUGUI components directly by searching children
                    // We know from inspection that TextMeshProUGUI exists on "Button Text Common" GameObjects
                    try
                    {
                        if (CKAccessibilityMod.DebugTextExtraction)
                            MelonLoader.MelonLogger.Msg("[GetButtonText] Searching for TextMeshProUGUI components in children...");
                        
                        // Search all child GameObjects for TextMeshProUGUI components
                        foreach (var childTransform in allTransforms)
                        {
                            Component[] childComps = childTransform.GetComponents<Component>();
                            foreach (var comp in childComps)
                            {
                                if (comp != null)
                                {
                                    var il2cppType = comp.GetIl2CppType();
                                    // Check if this is a TextMeshProUGUI component
                                    if (il2cppType.FullName == "TMPro.TextMeshProUGUI")
                                    {
                                        if (CKAccessibilityMod.DebugTextExtraction)
                                            MelonLoader.MelonLogger.Msg($"[GetButtonText] Found TextMeshProUGUI on {childTransform.gameObject.name}");
                                        
                                        // Use IL2CPP type to get the text property, not the managed wrapper type
                                        try
                                        {
                                            var textProp = il2cppType.GetProperty("text");
                                            if (textProp != null)
                                            {
                                                if (CKAccessibilityMod.DebugTextExtraction)
                                                    MelonLoader.MelonLogger.Msg($"[GetButtonText]   Found 'text' property on IL2CPP type");
                                                
                                                // GetValue needs to work with IL2CPP object
                                                var textValue = textProp.GetValue(comp, null);
                                                // Convert Il2CppSystem.Object to string
                                                string textString = textValue?.ToString();
                                                
                                                if (CKAccessibilityMod.DebugTextExtraction)
                                                    MelonLoader.MelonLogger.Msg($"[GetButtonText]   text value: \"{textString}\"");
                                                
                                                if (!string.IsNullOrWhiteSpace(textString))
                                                {
                                                    candidates.Add(textString);
                                                    if (CKAccessibilityMod.DebugTextExtraction)
                                                        MelonLoader.MelonLogger.Msg($"[GetButtonText] ✓ Extracted text: \"{textString}\" from {childTransform.gameObject.name}");
                                                }
                                            }
                                            else
                                            {
                                                if (CKAccessibilityMod.DebugTextExtraction)
                                                    MelonLoader.MelonLogger.Warning($"[GetButtonText]   'text' property NOT FOUND on IL2CPP type");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            if (CKAccessibilityMod.DebugTextExtraction)
                                                MelonLoader.MelonLogger.Warning($"[GetButtonText] Error extracting text from TMP: {ex.Message}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (CKAccessibilityMod.DebugTextExtraction)
                            MelonLoader.MelonLogger.Warning($"[GetButtonText] TextMeshProUGUI search error: {ex.Message}");
                    }

                    // Fallback: Try Unity UI Text components
                    if (candidates.Count == 0)
                    {
                        var uiTexts = monoBehaviour.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                        foreach (var uiText in uiTexts)
                        {
                            if (!string.IsNullOrWhiteSpace(uiText.text))
                            {
                                candidates.Add(uiText.text);
                                if (CKAccessibilityMod.DebugTextExtraction)
                                    MelonLoader.MelonLogger.Msg($"[GetButtonText] UI.Text component found: \"{uiText.text}\"");
                            }
                        }
                    }

                    // STRATEGY 3: Try STETextBlock components via reflection (game-specific component)
                    if (candidates.Count == 0)
                    {
                        var children = monoBehaviour.GetComponentsInChildren<MonoBehaviour>(true);
                        foreach (var child in children)
                        {
                            var childType = child.GetType();
                            if (childType.Name == "STETextBlock" || childType.Name.Contains("STEText"))
                            {
                                // Try properties: text, Text
                                var textProp = childType.GetProperty("text") ?? childType.GetProperty("Text");
                                if (textProp != null)
                                {
                                    var value = textProp.GetValue(child);
                                    if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                                    {
                                        candidates.Add(value.ToString());
                                        if (CKAccessibilityMod.DebugTextExtraction)
                                            MelonLoader.MelonLogger.Msg($"[GetButtonText] STETextBlock property: \"{value}\"");
                                    }
                                }
                                else
                                {
                                    // Try private fields: _text, _Text, m_text
                                    var textField = childType.GetField("_text", BindingFlags.NonPublic | BindingFlags.Instance)
                                        ?? childType.GetField("_Text", BindingFlags.NonPublic | BindingFlags.Instance)
                                        ?? childType.GetField("m_text", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (textField != null)
                                    {
                                        var value = textField.GetValue(child);
                                        if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                                        {
                                            candidates.Add(value.ToString());
                                            if (CKAccessibilityMod.DebugTextExtraction)
                                                MelonLoader.MelonLogger.Msg($"[GetButtonText] STETextBlock field: \"{value}\"");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Get canonical label
                var canonicalLabel = TextExtractionUtils.GetCanonicalLabel(candidates);

                if (CKAccessibilityMod.DebugTextExtraction)
                {
                    MelonLoader.MelonLogger.Msg($"[GetButtonText] Candidates: [{string.Join(", ", candidates.Select(c => $"\"{c}\""))}] -> Canonical: \"{canonicalLabel}\"");
                }

                // Fallback to cleaned GameObject name if no text found
                if (string.IsNullOrWhiteSpace(canonicalLabel) && monoBehaviour != null)
                {
                    var gameObjectName = monoBehaviour.gameObject.name;
                    gameObjectName = gameObjectName
                        .Replace("Button", "")
                        .Replace("UI/", "")
                        .Replace(" - #", " ")
                        .Replace("MainMenu", "Main Menu")
                        .Trim();
                    canonicalLabel = gameObjectName;
                    if (CKAccessibilityMod.DebugTextExtraction)
                        MelonLoader.MelonLogger.Msg($"[GetButtonText] Using fallback GO name: \"{canonicalLabel}\"");
                }
                
                return canonicalLabel;
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"[GetButtonText] Exception: {ex.Message}");
                return button.GetType().Name;
            }
        }

        private static string GetWidgetText(object widget)
        {
            if (widget == null) return string.Empty;

            try
            {
                var monoBehaviour = widget as MonoBehaviour;
                if (monoBehaviour == null) return string.Empty;

                var candidates = new List<string>();

                if (CKAccessibilityMod.DebugTextExtraction)
                    MelonLoader.MelonLogger.Msg($"[GetWidgetText] Widget type: {widget.GetType().FullName}, GO: {monoBehaviour.gameObject.name}");

                // PRIMARY: Try TextMeshPro components (IL2CPP compatible)
                var children = monoBehaviour.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var child in children)
                {
                    var childType = child.GetType();
                    if (childType.Name.Contains("TextMeshPro"))
                    {
                        var textProp = childType.GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
                        if (textProp != null)
                        {
                            var value = textProp.GetValue(child);
                            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                            {
                                candidates.Add(value.ToString());
                                if (CKAccessibilityMod.DebugTextExtraction)
                                    MelonLoader.MelonLogger.Msg($"[GetWidgetText] TextMeshPro found: \"{value}\" on {child.gameObject.name}");
                            }
                        }
                    }
                }

                // Fallback: Try STETextBlock components
                if (candidates.Count == 0)
                {
                    foreach (var child in children)
                    {
                        if (child.GetType().Name == "STETextBlock")
                        {
                            var textProp = child.GetType().GetProperty("text") ?? child.GetType().GetProperty("Text");
                            if (textProp != null)
                            {
                                var value = textProp.GetValue(child);
                                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                                {
                                    candidates.Add(value.ToString());
                                    if (CKAccessibilityMod.DebugTextExtraction)
                                        MelonLoader.MelonLogger.Msg($"[GetWidgetText] STETextBlock found: \"{value}\"");
                                }
                            }
                        }
                    }
                }

                // Try Unity UI Text
                if (candidates.Count == 0)
                {
                    var uiTexts = monoBehaviour.GetComponentsInChildren<UnityEngine.UI.Text>(true);
                    foreach (var uiText in uiTexts)
                    {
                        if (!string.IsNullOrWhiteSpace(uiText.text))
                        {
                            candidates.Add(uiText.text);
                            if (CKAccessibilityMod.DebugTextExtraction)
                                MelonLoader.MelonLogger.Msg($"[GetWidgetText] UI.Text found: \"{uiText.text}\"");
                        }
                    }
                }

                // Get canonical label
                var canonicalLabel = TextExtractionUtils.GetCanonicalLabel(candidates);

                if (CKAccessibilityMod.DebugTextExtraction)
                {
                    MelonLoader.MelonLogger.Msg($"[GetWidgetText] Candidates: [{string.Join(", ", candidates.Select(c => $"\"{c}\""))}] -> Canonical: \"{canonicalLabel}\"");
                }

                // Fallback to GameObject name
                if (string.IsNullOrWhiteSpace(canonicalLabel))
                {
                    canonicalLabel = monoBehaviour.gameObject.name;
                    if (CKAccessibilityMod.DebugTextExtraction)
                        MelonLoader.MelonLogger.Msg($"[GetWidgetText] Using fallback GO name: \"{canonicalLabel}\"");
                }

                return canonicalLabel;
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"[GetWidgetText] Exception: {ex.Message}");
                return widget.GetType().Name;
            }
        }

        // Catch-all Unity Selectable patch - handles generic Selectable objects that aren't STEButton
        public static void Unity_Selectable_OnSelect_Postfix(Selectable __instance, BaseEventData eventData)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceButtons) return;
                
                var type = __instance.GetType();
                var name = __instance.gameObject.name;
                
                // Skip if this is an STEButton or STEDefaultSliderWrapper (they have their own patches)
                if (type.Name == "STEButton" || type.Name == "STEDefaultSliderWrapper" || 
                    type.Name == "STEDialogAnswerButton" || type.Name == "STESelectInput")
                {
                    return;
                }
                
                MelonLoader.MelonLogger.Msg($"[DEBUG] Generic Selectable - Type: {type.Name}, GameObject: \"{name}\"");
                
                // Try to extract and announce text
                string text = "";
                
                // Special handling for attribute adjustment buttons
                if (name.Contains("Add Button") || name.Contains("Subtract Button"))
                {
                    // Find the attribute label by looking at parent/siblings
                    var parent = __instance.transform.parent;
                    if (parent != null)
                    {
                        // Look for text components in siblings
                        foreach (Transform sibling in parent)
                        {
                            var textComps = sibling.GetComponentsInChildren<UnityEngine.UI.Text>();
                            foreach (var tc in textComps)
                            {
                                if (!string.IsNullOrWhiteSpace(tc.text))
                                {
                                    var labelText = tc.text.Trim();
                                    // Check if it's an attribute name
                                    if (labelText == "Reaction" || labelText == "Strength" || labelText == "Will" || labelText == "Tech")
                                    {
                                        text = name.Contains("Add") ? $"Increase {labelText}" : $"Decrease {labelText}";
                                        goto foundLabel;
                                    }
                                }
                            }
                        }
                    }
                    foundLabel:;
                }
                
                // Try child text components if not a special button
                if (string.IsNullOrEmpty(text))
                {
                    // Try Unity UI Text first
                    var textComponents = __instance.GetComponentsInChildren<UnityEngine.UI.Text>();
                    if (textComponents != null && textComponents.Length > 0)
                    {
                        foreach (var tc in textComponents)
                        {
                            if (!string.IsNullOrWhiteSpace(tc.text))
                            {
                                text = tc.text;
                                break;
                            }
                        }
                    }
                    
                    // Try STETextBlock if Unity Text didn't work
                    if (string.IsNullOrEmpty(text))
                    {
                        text = GetWidgetText(__instance.gameObject);
                    }
                    
                    // Try cleaning up the game object name as last resort for tabs
                    if (string.IsNullOrEmpty(text) && name.Contains("Tab View"))
                    {
                        // Extract tab name from patterns like:
                        // "Weapons New Knight Tab View" -> "Weapons"
                        // "UI/Safehouse2/New Knight BackStory Tab View - #1" -> "BackStory"
                        text = name;
                        
                        // Remove path prefix if present
                        if (text.Contains("/"))
                        {
                            text = text.Substring(text.LastIndexOf("/") + 1);
                        }
                        
                        // Remove "New Knight" and "Tab View" parts
                        text = text.Replace("New Knight", "")
                                   .Replace("Tab View", "")
                                   .Trim();
                        
                        // Remove trailing pattern like " - #1"
                        if (text.Contains(" - #"))
                        {
                            text = text.Substring(0, text.IndexOf(" - #")).Trim();
                        }
                    }
                }
                
                // Clean up the text
                if (!string.IsNullOrWhiteSpace(text))
                {
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s*\(Corp Knowledge\)\s*", "").Trim();
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+\d+$", "").Trim(); // Remove trailing numbers
                    
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        MelonLoader.MelonLogger.Msg($"[Accessibility] Generic selectable focused: \"{text}\"");
                        SRALHelper.Speak(text, true);
                    }
                    else
                    {
                        MelonLoader.MelonLogger.Msg($"[DEBUG] Text was empty after cleanup: \"{name}\"");
                    }
                }
                else
                {
                    MelonLoader.MelonLogger.Msg($"[DEBUG] No text found for generic selectable: \"{name}\"");
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error in Unity_Selectable_OnSelect_Postfix: {ex.Message}");
            }
        }

        // Coroutine to delay text announcement until typing animation completes
        private static System.Collections.IEnumerator DelayedAnnounce(string text, int delayMs)
        {
            yield return new UnityEngine.WaitForSeconds(delayMs / 1000f);
            SRALHelper.Speak(text, false);
        }

        // Coroutine to enter attribute screen after a delay to allow UI to settle
        private static System.Collections.IEnumerator EnterAttributeScreenDelayed()
        {
            yield return new UnityEngine.WaitForSeconds(0.5f);
            CharacterCreationNavigation.EnterAttributeScreen();
        }
    }
}
