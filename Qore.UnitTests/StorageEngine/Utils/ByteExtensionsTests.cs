using FluentAssertions;
using QoreDB.StorageEngine;
using System.Text;

namespace Qore.UnitTests.StorageEngine.Utils
{
    [TestFixture]
    public class ByteExtensionsTests
    {
        [Test]
        public void ToHexString_WithNullArray_ShouldReturnNullPlaceholder()
        {
            // Arrange
            byte[] data = null;

            // Act
            string result = data.ToHexDump();

            // Assert
            result.Should().Be("<null>");
        }

        [Test]
        public void ToHexString_WithEmptyArray_ShouldReturnEmptyPlaceholder()
        {
            // Arrange
            byte[] data = Array.Empty<byte>();

            // Act
            string result = data.ToHexDump();

            // Assert
            result.Should().Be("<empty>");
        }

        [Test]
        public void ToHexString_WithSingleFullLine_ShouldFormatCorrectly()
        {
            // Arrange
            byte[] data = Encoding.ASCII.GetBytes("0123456789ABCDEF"); // 16 bytes exactly

            // Act
            string result = data.ToHexDump();

            // Assert
            string[] lines = result.Split(Environment.NewLine);
            lines.Should().HaveCount(3); // Header, Separator, Data Line
            lines[0].Should().StartWith("Offset(h)");
            lines[1].Should().MatchRegex("^-+$"); // Should be a line of dashes
            lines[2].Should().Be("00000000   30 31 32 33 34 35 36 37  38 39 41 42 43 44 45 46   0123456789ABCDEF");
        }

        [Test]
        public void ToHexString_WithPartialLastLine_ShouldPadHexPartCorrectly()
        {
            // Arrange
            byte[] data = Encoding.ASCII.GetBytes("This is twenty bytes"); // 20 bytes
            const int hexDisplayWidth = 49; // For 16 bytes/line: (16 * 3 chars) + 1 group separator

            // Act
            string result = data.ToHexDump();

            // Assert
            string[] lines = result.Split(Environment.NewLine);
            lines.Should().HaveCount(4); // Header, Separator, Line 1, Line 2

            // Check the second data line (the partial one)
            string partialLine = lines[3];
            partialLine.Should().StartWith("00000010   ");
            partialLine.Should().EndWith("  ytes"); // 2 spaces before ASCII part

            // Isolate and check the hex part for correct content and padding
            string hexPart = partialLine.Substring(11, hexDisplayWidth);
            hexPart.Should().StartWith("79 74 65 73");
            hexPart.TrimEnd().Should().Be("79 74 65 73"); // Content should be correct
            hexPart.Length.Should().Be(hexDisplayWidth, "the hex part must be padded to maintain alignment");
        }

        [Test]
        public void ToHexString_WithControlCharacters_ShouldReplaceThemWithDotsInAscii()
        {
            // Arrange
            // "Hello,\tWorld!\0" (includes tab and null terminator)
            byte[] data = { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x2C, 0x09, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x21, 0x00 };

            // Act
            string result = data.ToHexDump();

            // Assert
            string dataLine = result.Split(Environment.NewLine).Last();
            dataLine.Should().EndWith("  Hello,.World!."); // \t and \0 replaced with '.'
        }

        [Test]
        public void ToHexString_WithCustomBytesPerLine_ShouldFormatAccordingToCustomWidth()
        {
            // Arrange
            byte[] data = Enumerable.Range(0, 10).Select(b => (byte)b).ToArray();
            const int bytesPerLine = 8;
            const int hexDisplayWidth = 25; // For 8 bytes/line: (8 * 3 chars) + 1 group separator

            // Act
            string result = data.ToHexDump(bytesPerLine: bytesPerLine);

            // Assert
            string[] lines = result.Split(Environment.NewLine);
            lines.Should().HaveCount(4);

            // Check header
            lines[0].Should().Be("Offset(h)  00 01 02 03  04 05 06 07  Decoded Text");
            lines[1].Should().Be("-------------------------------------------------");

            // Check full data line
            lines[2].Should().Be("00000000   00 01 02 03  04 05 06 07   ........");

            // Check partial data line
            string partialLine = lines[3];
            string hexPart = partialLine.Substring(11, hexDisplayWidth);
            hexPart.Should().StartWith("08 09");
            hexPart.Length.Should().Be(hexDisplayWidth, "the hex part must be padded");
            partialLine.Should().EndWith("  ..");
        }
    }
}