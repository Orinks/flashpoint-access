using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CKFlashpointAccessibility
{
    /// <summary>
    /// Pure utility class for normalizing and aggregating text from multiple UI sources.
    /// Designed to be testable without game dependencies.
    /// </summary>
    public static class TextExtractionUtils
    {
        /// <summary>
        /// Normalizes a collection of candidate text strings into a single canonical label.
        /// Prioritizes longer, non-empty strings, joins with spaces, and cleans formatting.
        /// </summary>
        /// <param name="candidates">List of raw text candidates from various sources (e.g., _Text, _SubText, child components)</param>
        /// <returns>Normalized label string, or empty if no valid candidates</returns>
        public static string GetCanonicalLabel(IEnumerable<string> candidates)
        {
            if (candidates == null)
                return string.Empty;

            var validCandidates = candidates
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(NormalizeText)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            if (!validCandidates.Any())
                return string.Empty;

            // Join with single space, collapse multiple spaces
            var joined = string.Join(" ", validCandidates);
            return Regex.Replace(joined, @"\s+", " ").Trim();
        }

        /// <summary>
        /// Normalizes a single text string: strips TMP tags, removes zero-width/control chars, collapses whitespace.
        /// </summary>
        /// <param name="text">Raw text input</param>
        /// <returns>Normalized text</returns>
        public static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Strip TMP tags like <b>, </color>, etc.
            text = Regex.Replace(text, @"<[^>]+>", string.Empty);

            // Collapse multiple whitespace to single space
            text = Regex.Replace(text, @"\s+", " ");

            // Remove zero-width spaces, control characters, non-breaking spaces
            text = Regex.Replace(text, @"[\u200B\u00A0\u0000-\u001F\u007F-\u009F]", string.Empty);

            // Trim punctuation-only strings (e.g., "!!" -> "")
            text = text.Trim();
            if (Regex.IsMatch(text, @"^[\p{P}\p{S}]+$"))
                return string.Empty;

            return text;
        }

        /// <summary>
        /// Checks if a normalized label is suitable for speech (not single char unless it's a letter/digit).
        /// </summary>
        /// <param name="label">Normalized label</param>
        /// <returns>True if safe to speak</returns>
        public static bool IsSpeakableLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
                return false;

            // Allow single letters/digits (e.g., "A" for menu items), but not punctuation
            if (label.Length == 1)
                return char.IsLetterOrDigit(label[0]);

            return true;
        }
    }
}