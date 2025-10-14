using System.Text;

namespace QoreDB.StorageEngine
{
    /// <summary>
    /// Provides extension methods for byte arrays.
    /// </summary>
    public static class ByteExtensions
    {
        /// <summary>
        /// Generates a string representation of a byte array formatted like a hex editor,
        /// using LINQ for a cleaner, more declarative implementation.
        /// </summary>
        /// <remarks>
        /// This method requires .NET 6 or later for the Enumerable.Chunk() method.
        /// </remarks>
        /// <param name="data">The byte array to format.</param>
        /// <param name="bytesPerLine">The number of bytes to display on a single line. Defaults to 16.</param>
        /// <returns>A string formatted as a hex dump.</returns>
        public static string ToHexDump(this byte[] data, int bytesPerLine = 16)
        {
            // 1. Handle edge cases
            if (data == null) return "<null>";
            if (data.Length == 0) return "<empty>";

            // 2. Define the layout
            int groupSize = bytesPerLine / 2;
            // Calculate the total width of the hex character display
            int hexWidth = bytesPerLine * 3 + (bytesPerLine / groupSize) - 1;

            // 3. Build the header row
            var headerBuilder = new StringBuilder("Offset(h)  ");
            for (int i = 0; i < bytesPerLine; i++)
            {
                if (i > 0 && i % groupSize == 0) headerBuilder.Append(' ');
                headerBuilder.Append($"{i:X2} ");
            }
            string header = headerBuilder.Append(" Decoded Text").ToString();
            string separator = new('-', header.Length);

            // 4. Use LINQ to process the data in chunks (lines)
            var body = data
                .Chunk(bytesPerLine) // 👈 Split the data into lines of `bytesPerLine`
                .Select((lineData, index) =>
                {
                    int offset = index * bytesPerLine;

                    // Use LINQ to create the hex part, with a double space for the group separator
                    string hexPart = string.Join("  ", lineData
                        .Select(b => $"{b:X2}") // Convert each byte to a two-digit hex string
                        .Chunk(groupSize)      // Group hex strings for the separator
                        .Select(g => string.Join(" ", g)))
                        .PadRight(hexWidth);   // Pad to align columns

                    // Use LINQ to create the ASCII part, replacing control chars with '.'
                    string asciiPart = new string(lineData
                        .Select(b => char.IsControl((char)b) ? '.' : (char)b)
                        .ToArray());

                    // Combine all parts into the final line string
                    return $"{offset:X8}   {hexPart}  {asciiPart}";
                });

            // 5. Combine header, separator, and all body lines
            return $"{header}{Environment.NewLine}{separator}{Environment.NewLine}{string.Join(Environment.NewLine, body)}";
        }
    }
}
