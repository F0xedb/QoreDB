using QoreDB.Catalog.Models;
using QoreDB.StorageEngine.Index.Interfaces;
using System;
using System.IO;
using System.Text;

namespace QoreDB.StorageEngine.Index.Serializer
{
    /// <summary>
    /// Handles serialization and deserialization for ColumnInfo objects
    /// </summary>
    public class ColumnInfoSerializer : ISerializer<ColumnInfo>
    {
        public byte[] Serialize(ColumnInfo value)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, false);

            writer.Write(value.ColumnName);
            writer.Write(value.DataType.FullName);

            return stream.ToArray();
        }

        public ColumnInfo Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream, Encoding.UTF8, false);

            var columnName = reader.ReadString();
            var typeName = reader.ReadString();
            var type = Type.GetType(typeName)
                ?? throw new InvalidDataException($"Could not deserialize type '{typeName}' for column '{columnName}'");

            return new ColumnInfo(columnName, type);
        }
    }
}