using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        public static void STETextBlock_SetText_Postfix(object __instance, string text)
        {
            try
            {
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

                if (!string.IsNullOrEmpty(text))
                {
                    SRALHelper.Speak(text, false);
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

                string text = GetWidgetText(__instance);
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
                if (!CKAccessibilityMod.AnnounceMenuItems) return;

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
                MelonLoader.MelonLogger.Msg($"[GetButtonText] Button type: {buttonType.FullName}");
                
                // Try accessing _Text field directly via reflection (from IL2CPP dump analysis)
                var textField = buttonType.GetField("_Text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (textField != null)
                {
                    var textComponent = textField.GetValue(button);
                    MelonLoader.MelonLogger.Msg($"[GetButtonText] _Text field value: {textComponent?.GetType().Name ?? "null"}");
                    
                    if (textComponent != null)
                    {
                        // Try to get the text property
                        var textProp = textComponent.GetType().GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
                        if (textProp != null)
                        {
                            var textValue = textProp.GetValue(textComponent);
                            if (textValue != null)
                            {
                                var textStr = textValue.ToString();
                                MelonLoader.MelonLogger.Msg($"[GetButtonText] Found text via _Text field: \"{textStr}\"");
                                if (!string.IsNullOrEmpty(textStr))
                                    return textStr;
                            }
                        }
                    }
                }
                
                // Try _SubText field
                var subTextField = buttonType.GetField("_SubText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (subTextField != null)
                {
                    var textComponent = subTextField.GetValue(button);
                    if (textComponent != null)
                    {
                        var textProp = textComponent.GetType().GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
                        if (textProp != null)
                        {
                            var textValue = textProp.GetValue(textProp);
                            if (textValue != null && !string.IsNullOrEmpty(textValue.ToString()))
                            {
                                MelonLoader.MelonLogger.Msg($"[GetButtonText] Found text via _SubText: \"{textValue}\"");
                                return textValue.ToString();
                            }
                        }
                    }
                }
                
                // Fallback: Try GameObject name cleanup
                var monoBehaviour = button as MonoBehaviour;
                if (monoBehaviour != null)
                {
                    var gameObjectName = monoBehaviour.gameObject.name;
                    // Clean up GameObject names for better speech
                    gameObjectName = gameObjectName
                        .Replace("Button", "")
                        .Replace("UI/", "")
                        .Replace(" - #", " ")
                        .Replace("MainMenu", "Main Menu")
                        .Trim();
                    
                    MelonLoader.MelonLogger.Msg($"[GetButtonText] Using cleaned GameObject name: \"{gameObjectName}\"");
                    return gameObjectName;
                }
                
                return "Button";
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

                // Try to find text components using reflection
                var children = monoBehaviour.GetComponentsInChildren<MonoBehaviour>();
                foreach (var child in children)
                {
                    if (child.GetType().Name == "STETextBlock")
                    {
                        var textProp = child.GetType().GetProperty("text") ?? child.GetType().GetProperty("Text");
                        if (textProp != null)
                        {
                            var value = textProp.GetValue(child);
                            if (value != null) return value.ToString();
                        }
                    }
                }

                // Try TextMeshPro
                foreach (var child in children)
                {
                    if (child.GetType().Name.Contains("TextMeshPro"))
                    {
                        var textProp = child.GetType().GetProperty("text");
                        if (textProp != null)
                        {
                            var value = textProp.GetValue(child);
                            if (value != null && !string.IsNullOrEmpty(value.ToString()))
                                return value.ToString();
                        }
                    }
                }

                // Fallback to GameObject name
                return monoBehaviour.gameObject.name;
            }
            catch
            {
                return widget.GetType().Name;
            }
        }
    }
}
