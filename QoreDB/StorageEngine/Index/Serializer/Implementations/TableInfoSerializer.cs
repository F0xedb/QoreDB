using QoreDB.Catalog.Models;
using QoreDB.StorageEngine.Index.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QoreDB.StorageEngine.Index.Serializer
{
    /// <summary>
    /// Handles serialization and deserialization for TableInfo objects
    /// </summary>
    public class TableInfoSerializer : ISerializer<TableInfo>
    {
        public byte[] Serialize(TableInfo value)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, false);

            writer.Write(value.TableName);
            writer.Write(value.RootPage);
            writer.Write(value.Columns.Count);

            foreach (var column in value.Columns)
            {
                writer.Write(column.ColumnName);
                writer.Write(column.DataType.FullName);
            }

            return stream.ToArray();
        }

        public TableInfo Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream, Encoding.UTF8, false);

            var tableName = reader.ReadString();
            var rootPage = reader.ReadInt32();
            var columnCount = reader.ReadInt32();
            var columns = new List<ColumnInfo>(columnCount);

            for (int i = 0; i < columnCount; i++)
            {
                var columnName = reader.ReadString();
                var typeName = reader.ReadString();
                var type = Type.GetType(typeName)
                    ?? throw new InvalidDataException($"Could not deserialize type '{typeName}' for column '{columnName}'");

                columns.Add(new ColumnInfo(columnName, type));
            }

            return new TableInfo(tableName, columns, rootPage);
        }
    }
}