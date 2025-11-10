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
                    SRALHelper.Speak($"{text}", true);
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
                    SRALHelper.Speak(t, false);
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
    }
}
