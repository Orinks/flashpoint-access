using System.Collections.Generic;
using Xunit;
using CKFlashpointAccessibility;

namespace TextExtraction.Tests
{
    public class TextExtractionUtilsTests
    {
        [Fact]
        public void GetCanonicalLabel_EmptyInput_ReturnsEmpty()
        {
            var result = TextExtractionUtils.GetCanonicalLabel(new List<string>());
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetCanonicalLabel_NullInput_ReturnsEmpty()
        {
            var result = TextExtractionUtils.GetCanonicalLabel(null);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetCanonicalLabel_SingleCandidate_ReturnsNormalized()
        {
            var candidates = new List<string> { "  Hello  " };
            var result = TextExtractionUtils.GetCanonicalLabel(candidates);
            Assert.Equal("Hello", result);
        }

        [Fact]
        public void GetCanonicalLabel_MultipleCandidates_JoinsWithSpace()
        {
            var candidates = new List<string> { "Menu", "Item" };
            var result = TextExtractionUtils.GetCanonicalLabel(candidates);
            Assert.Equal("Menu Item", result);
        }

        [Fact]
        public void GetCanonicalLabel_SplitsText_JoinsFragments()
        {
            var candidates = new List<string> { "M", "enu" };
            var result = TextExtractionUtils.GetCanonicalLabel(candidates);
            Assert.Equal("M enu", result); // Should join as-is, normalization handles spaces
        }

        [Fact]
        public void GetCanonicalLabel_WithTMPTags_StripsTags()
        {
            var candidates = new List<string> { "<b>Load</b>", "Game" };
            var result = TextExtractionUtils.GetCanonicalLabel(candidates);
            Assert.Equal("Load Game", result);
        }

        [Fact]
        public void GetCanonicalLabel_CollapsesWhitespace()
        {
            var candidates = new List<string> { "Load\n\nGame", "  Extra  " };
            var result = TextExtractionUtils.GetCanonicalLabel(candidates);
            Assert.Equal("Load Game Extra", result);
        }

        [Fact]
        public void GetCanonicalLabel_JoinsInProvidedOrder()
        {
            var candidates = new List<string> { "A", "Menu", "M" };
            var result = TextExtractionUtils.GetCanonicalLabel(candidates);
            // Joins in the order provided, distinct
            Assert.Equal("A Menu M", result);
        }

        [Fact]
        public void GetCanonicalLabel_RemovesDuplicates()
        {
            var candidates = new List<string> { "Menu", "Menu", "Item" };
            var result = TextExtractionUtils.GetCanonicalLabel(candidates);
            Assert.Equal("Menu Item", result);
        }

        [Fact]
        public void GetCanonicalLabel_IgnoresEmptyAndWhitespace()
        {
            var candidates = new List<string> { "", "  ", "Menu", null };
            var result = TextExtractionUtils.GetCanonicalLabel(candidates);
            Assert.Equal("Menu", result);
        }

        [Fact]
        public void GetCanonicalLabel_PunctuationOnly_Ignored()
        {
            var candidates = new List<string> { "!!", "Menu" };
            var result = TextExtractionUtils.GetCanonicalLabel(candidates);
            Assert.Equal("Menu", result);
        }

        [Fact]
        public void NormalizeText_StripsTMPTags()
        {
            var input = "<color=red>Hello</color> <b>World</b>";
            var result = TextExtractionUtils.NormalizeText(input);
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void NormalizeText_RemovesZeroWidthChars()
        {
            var input = "Menu\u200BItem"; // Zero-width space
            var result = TextExtractionUtils.NormalizeText(input);
            Assert.Equal("MenuItem", result);
        }

        [Fact]
        public void NormalizeText_CollapsesWhitespace()
        {
            var input = "Menu\n\t  Item";
            var result = TextExtractionUtils.NormalizeText(input);
            Assert.Equal("Menu Item", result);
        }

        [Fact]
        public void NormalizeText_HandlesNewlines()
        {
            var input = "Load\n\nGame";
            var result = TextExtractionUtils.NormalizeText(input);
            Assert.Equal("Load Game", result);
        }

        [Fact]
        public void IsSpeakableLabel_ValidLabels_ReturnTrue()
        {
            Assert.True(TextExtractionUtils.IsSpeakableLabel("Menu"));
            Assert.True(TextExtractionUtils.IsSpeakableLabel("A"));
            Assert.True(TextExtractionUtils.IsSpeakableLabel("1"));
        }

        [Fact]
        public void IsSpeakableLabel_InvalidLabels_ReturnFalse()
        {
            Assert.False(TextExtractionUtils.IsSpeakableLabel(""));
            Assert.False(TextExtractionUtils.IsSpeakableLabel("   "));
            Assert.False(TextExtractionUtils.IsSpeakableLabel("!"));
        }
    }
}