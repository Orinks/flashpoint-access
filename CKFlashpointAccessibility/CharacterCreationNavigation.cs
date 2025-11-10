using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CKFlashpointAccessibility
{
    /// <summary>
    /// Custom keyboard/controller navigation for character creation screens
    /// Provides accessible attribute adjustment and screen navigation
    /// Static class - no MonoBehaviour needed for IL2CPP compatibility
    /// </summary>
    public static class CharacterCreationNavigation
    {
        private static bool _isOnAttributeScreen = false;
        private static int _currentAttributeIndex = 0;
        private static List<AttributeRow> _attributeRows = new List<AttributeRow>();
        private static float _lastKeyPressTime = 0f;
        private const float KEY_REPEAT_DELAY = 0.15f; // 150ms between repeats

        private class AttributeRow
        {
            public string AttributeName;
            public Selectable IncreaseButton;
            public Selectable DecreaseButton;
            public Text ValueText;
            public Text NameText;
        }

        // Called from UIPatches Update to check for input
        public static void Update()
        {
            if (!_isOnAttributeScreen || _attributeRows.Count == 0)
            {
                return;
            }

            try
            {
                // Throttle key repeats
                if (Time.time - _lastKeyPressTime < KEY_REPEAT_DELAY)
                    return;

                // Cycle between attributes with Q/E or LB/RB
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    MelonLoader.MelonLogger.Msg("[CharCreation] Q key pressed");
                    CyclePreviousAttribute();
                    _lastKeyPressTime = Time.time;
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    MelonLoader.MelonLogger.Msg("[CharCreation] E key pressed");
                    CycleNextAttribute();
                    _lastKeyPressTime = Time.time;
                }
                // Adjust current attribute with Left/Right arrows (use GetKeyDown for discrete input)
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    MelonLoader.MelonLogger.Msg("[CharCreation] Left arrow pressed");
                    DecreaseCurrentAttribute();
                    _lastKeyPressTime = Time.time;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    MelonLoader.MelonLogger.Msg("[CharCreation] Right arrow pressed");
                    IncreaseCurrentAttribute();
                    _lastKeyPressTime = Time.time;
                }
                // Read current attribute state with Space
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    MelonLoader.MelonLogger.Msg("[CharCreation] Space pressed");
                    AnnounceCurrentAttribute();
                    _lastKeyPressTime = Time.time;
                }
                // Controller support
                else if (Input.GetKeyDown(KeyCode.JoystickButton4)) // LB
                {
                    MelonLoader.MelonLogger.Msg("[CharCreation] LB pressed");
                    CyclePreviousAttribute();
                    _lastKeyPressTime = Time.time;
                }
                else if (Input.GetKeyDown(KeyCode.JoystickButton5)) // RB
                {
                    MelonLoader.MelonLogger.Msg("[CharCreation] RB pressed");
                    CycleNextAttribute();
                    _lastKeyPressTime = Time.time;
                }
                else if (Input.GetKeyDown(KeyCode.JoystickButton0)) // A button
                {
                    MelonLoader.MelonLogger.Msg("[CharCreation] A button pressed");
                    AnnounceCurrentAttribute();
                    _lastKeyPressTime = Time.time;
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"[CharCreation] Error in Update: {ex.Message}");
            }
        }

        public static void EnterAttributeScreen()
        {
            MelonLoader.MelonLogger.Msg("[CharCreation] EnterAttributeScreen called");
            _isOnAttributeScreen = true;
            _currentAttributeIndex = 0;
            ScanForAttributeRows();
            
            if (_attributeRows.Count > 0)
            {
                MelonLoader.MelonLogger.Msg($"[CharCreation] Detected {_attributeRows.Count} attribute rows - entering navigation mode");
                SRALHelper.Speak("Attribute screen. Use Q and E to cycle attributes, Left and Right arrows to adjust, Space to read current value.", false);
                AnnounceCurrentAttribute();
            }
            else
            {
                MelonLoader.MelonLogger.Warning("[CharCreation] No attribute rows found!");
                _isOnAttributeScreen = false; // Don't stay in attribute mode if no rows found
            }
        }

        public static void ExitAttributeScreen()
        {
            _isOnAttributeScreen = false;
            _attributeRows.Clear();
            _currentAttributeIndex = 0;
            MelonLoader.MelonLogger.Msg("[CharCreation] Exited attribute screen");
        }

        private static void ScanForAttributeRows()
        {
            _attributeRows.Clear();

            try
            {
                // Find all GameObjects including inactive ones
                var allObjects = UnityEngine.Object.FindObjectsOfType<UnityEngine.GameObject>(true);
                
                var addButtons = new List<Selectable>();
                var subtractButtons = new List<Selectable>();

                foreach (var obj in allObjects)
                {
                    var name = obj.name;
                    if (name.Contains("Button") && (name.Contains("Add") || name.Contains("Subtract")))
                    {
                        var selectable = obj.GetComponent<Selectable>();
                        if (selectable != null)
                        {
                            if (name.Contains("Add"))
                                addButtons.Add(selectable);
                            else if (name.Contains("Subtract"))
                                subtractButtons.Add(selectable);
                        }
                    }
                }

                // Match buttons to attributes
                foreach (var addBtn in addButtons)
                {
                    var subtractBtn = FindMatchingSubtractButton(addBtn, subtractButtons);
                    if (subtractBtn != null)
                    {
                        var attributeName = FindAttributeName(addBtn);
                        if (!string.IsNullOrEmpty(attributeName))
                        {
                            var row = new AttributeRow
                            {
                                AttributeName = attributeName,
                                IncreaseButton = addBtn,
                                DecreaseButton = subtractBtn
                            };
                            _attributeRows.Add(row);
                        }
                    }
                }
                
                MelonLoader.MelonLogger.Msg($"[CharCreation] Found {_attributeRows.Count} attribute rows");

                // Sort by attribute order: Reaction, Strength, Will, Tech
                _attributeRows.Sort((a, b) => GetAttributeOrder(a.AttributeName).CompareTo(GetAttributeOrder(b.AttributeName)));
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"[CharCreation] Error scanning attribute rows: {ex.Message}");
            }
        }

        private static Selectable FindMatchingSubtractButton(Selectable addButton, List<Selectable> subtractButtons)
        {
            var addParent = addButton.transform.parent;
            foreach (var subBtn in subtractButtons)
            {
                if (subBtn.transform.parent == addParent)
                    return subBtn;
            }
            return null;
        }

        private static string FindAttributeName(Selectable button)
        {
            var parent = button.transform.parent;
            
            for (int i = 0; i < 5 && parent != null; i++)
            {
                var textComponents = parent.GetComponentsInChildren<Text>(true);
                
                foreach (var text in textComponents)
                {
                    var label = text.text.Trim();
                    if (label == "Reaction" || label == "Strength" || label == "Will" || label == "Tech")
                        return label;
                }
                parent = parent.parent;
            }
            return null;
        }

        private static int GetAttributeOrder(string attributeName)
        {
            switch (attributeName)
            {
                case "Reaction": return 0;
                case "Strength": return 1;
                case "Will": return 2;
                case "Tech": return 3;
                default: return 99;
            }
        }

        private static void CycleNextAttribute()
        {
            if (_attributeRows.Count == 0) return;
            
            _currentAttributeIndex = (_currentAttributeIndex + 1) % _attributeRows.Count;
            AnnounceCurrentAttribute();
        }

        private static void CyclePreviousAttribute()
        {
            if (_attributeRows.Count == 0) return;
            
            _currentAttributeIndex--;
            if (_currentAttributeIndex < 0)
                _currentAttributeIndex = _attributeRows.Count - 1;
            AnnounceCurrentAttribute();
        }

        private static void IncreaseCurrentAttribute()
        {
            if (_currentAttributeIndex >= _attributeRows.Count) return;
            
            var row = _attributeRows[_currentAttributeIndex];
            if (row.IncreaseButton != null && row.IncreaseButton.interactable)
            {
                // Simulate pointer click on Selectable
                var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                UnityEngine.EventSystems.ExecuteEvents.Execute(row.IncreaseButton.gameObject, pointerData, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
                
                MelonLoader.MelonLogger.Msg($"[CharCreation] Increased {row.AttributeName}");
                
                // Announce the change after a short delay to get updated value
                MelonLoader.MelonCoroutines.Start(AnnounceAttributeAfterDelay(0.1f));
            }
            else
            {
                SRALHelper.Speak("Maximum reached", true);
            }
        }

        private static void DecreaseCurrentAttribute()
        {
            if (_currentAttributeIndex >= _attributeRows.Count) return;
            
            var row = _attributeRows[_currentAttributeIndex];
            if (row.DecreaseButton != null && row.DecreaseButton.interactable)
            {
                // Simulate pointer click on Selectable
                var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                UnityEngine.EventSystems.ExecuteEvents.Execute(row.DecreaseButton.gameObject, pointerData, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
                
                MelonLoader.MelonLogger.Msg($"[CharCreation] Decreased {row.AttributeName}");
                
                // Announce the change after a short delay to get updated value
                MelonLoader.MelonCoroutines.Start(AnnounceAttributeAfterDelay(0.1f));
            }
            else
            {
                SRALHelper.Speak("Minimum reached", true);
            }
        }

        private static void AnnounceCurrentAttribute()
        {
            if (_currentAttributeIndex >= _attributeRows.Count) return;
            
            var row = _attributeRows[_currentAttributeIndex];
            
            // Try to find the current value and max
            int currentValue = GetAttributeValue(row);
            int freePoints = GetFreePoints();
            
            string announcement = $"{row.AttributeName}, {currentValue}";
            if (freePoints >= 0)
                announcement += $", {freePoints} points available";
            
            SRALHelper.Speak(announcement, true);
            MelonLoader.MelonLogger.Msg($"[CharCreation] Announced: {announcement}");
        }

        private static int GetAttributeValue(AttributeRow row)
        {
            // Try to find value text near the buttons
            var parent = row.IncreaseButton.transform.parent;
            if (parent != null)
            {
                var textComponents = parent.GetComponentsInChildren<Text>();
                foreach (var text in textComponents)
                {
                    // Look for numeric value (0-6 typically)
                    if (int.TryParse(text.text.Trim(), out int value) && value >= 0 && value <= 10)
                    {
                        return value;
                    }
                }
            }
            return 0;
        }

        private static int GetFreePoints()
        {
            // Find "Free points:" text
            var allText = UnityEngine.Object.FindObjectsOfType<Text>();
            foreach (var text in allText)
            {
                if (text.text.Contains("Free points:"))
                {
                    // Extract number from markup like "<size=62%>Free points: </size><color=#FFF223FF>12</color>"
                    var match = System.Text.RegularExpressions.Regex.Match(text.text, @">(\d+)<");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int points))
                    {
                        return points;
                    }
                }
            }
            return -1;
        }

        private static System.Collections.IEnumerator AnnounceAttributeAfterDelay(float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            AnnounceCurrentAttribute();
        }
    }
}

