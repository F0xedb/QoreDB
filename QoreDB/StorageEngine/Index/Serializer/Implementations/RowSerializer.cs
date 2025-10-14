using QoreDB.Catalog.Models;
using QoreDB.StorageEngine.Index.Interfaces;
using System.Text;

namespace QoreDB.StorageEngine.Index.Serializer.Implementations
{
    /// <summary>
    /// Serializes and deserializes table rows based on a given schema
    /// </summary>
    public class RowSerializer
    {
        /// <summary>
        /// Serializes a dictionary of row data into a byte array
        /// </summary>
        public byte[] Serialize(TableInfo tableSchema, Dictionary<string, object> row)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            foreach (var column in tableSchema.Columns)
            {
                if (!row.TryGetValue(column.ColumnName, out var value))
                {
                    throw new ArgumentException($"Missing value for column '{column.ColumnName}'");
                }

                if (value is null)
                {
                    // Handle nulls with a simple flag
                    writer.Write(false);
                    continue;
                }

                writer.Write(true); // Not null

                if (column.DataType == typeof(int))
                {
                    writer.Write((int)value);
                }
                else if (column.DataType == typeof(string))
                {
                    writer.Write((string)value);
                }
                // Add more type handlers here (long, double, bool, etc)
                else
                {
                    throw new NotSupportedException($"Type '{column.DataType.Name}' is not supported");
                }
            }

            return stream.ToArray();
        }
        
        /// <summary>
        /// Deserializes a byte array into a dictionary representing a row
        /// </summary>
        public Dictionary<string, object> Deserialize(TableInfo tableSchema, byte[] data)
        {
            var row = new Dictionary<string, object>();
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            foreach (var column in tableSchema.Columns)
            {
                var isNotNull = reader.ReadBoolean();
                if (!isNotNull)
                {
                    row[column.ColumnName] = null;
                    continue;
                }

                if (column.DataType == typeof(int))
                {
                    row[column.ColumnName] = reader.ReadInt32();
                }
                else if (column.DataType == typeof(string))
                {
                    row[column.ColumnName] = reader.ReadString();
                }
                // Add more type handlers here as you support them
                else
                {
                    throw new NotSupportedException($"Type '{column.DataType.Name}' is not supported");
                }
            }
            return row;
        }
    }
}
