using System;
using System.IO;

using ProjectXenocide.Services;

using Xunit;

namespace Xenocide.Test.MonoGame
{
    /// <summary>
    /// Tests for save-file naming and the .xsv extension handling.
    /// </summary>
    public class SavegameServiceTests
    {
        [Theory]
        [InlineData("MySave", "MySave.xsv")]
        [InlineData("MySave.xsv", "MySave.xsv")]
        [InlineData("MySave.XSV", "MySave.xsv")]
        [InlineData("  Padded  ", "Padded.xsv")]
        public void NormalizeName_AppendsExtensionExactlyOnce(string input, string expected)
        {
            Assert.Equal(expected, SavegameService.NormalizeName(input));
        }

        [Fact]
        public void NormalizeName_ReplacesInvalidFileNameCharacters()
        {
            string invalid = new string(Path.GetInvalidFileNameChars()[0], 1);

            string result = SavegameService.NormalizeName("a" + invalid + "b");

            Assert.Equal("a_b.xsv", result);
        }

        [Fact]
        public void NormalizeName_EmptyInputGetsFallbackName()
        {
            Assert.Equal("save.xsv", SavegameService.NormalizeName("   "));
        }

        [Theory]
        [InlineData("MySave.xsv", "MySave")]
        [InlineData("MySave.XSV", "MySave")]
        [InlineData("noext", "noext")]
        public void StripExtension_RemovesOnlyTheSaveExtension(string input, string expected)
        {
            Assert.Equal(expected, SavegameService.StripExtension(input));
        }

        [Fact]
        public void GenerateDefaultName_UsesSaveExtension()
        {
            string name = SavegameService.GenerateDefaultName();

            Assert.EndsWith(SavegameService.Extension, name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
