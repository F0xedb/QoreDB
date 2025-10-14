using FluentAssertions;
using NUnit.Framework;
using QoreDB.Catalog.Models;
using QoreDB.StorageEngine.Index.Serializer;
using QoreDB.StorageEngine.Index.Serializer.Implementations;
using System;
using System.Collections.Generic;
using System.IO;

namespace Qore.UnitTests.StorageEngine.Serializer
{
    [TestFixture]
    public class RowSerializerTests
    {
        [Test]
        public void Serialize_WithValidData_ShouldProduceCorrectByteArray()
        {
            // Arrange
            var serializer = new RowSerializer();
            var columns = new List<ColumnInfo>
            {
                new ColumnInfo("Id", typeof(int)),
                new ColumnInfo("Name", typeof(string)),
                new ColumnInfo("Email", typeof(string))
            };
            var tableInfo = new TableInfo("Users", columns, 1);
            var row = new Dictionary<string, object>
            {
                { "Id", 123 },
                { "Name", "John Doe" },
                { "Email", null }
            };

            // Act
            var serializedRow = serializer.Serialize(tableInfo, row);

            // Assert
            using var stream = new MemoryStream(serializedRow);
            using var reader = new BinaryReader(stream);

            reader.ReadBoolean().Should().BeTrue(); // Id is not null
            reader.ReadInt32().Should().Be(123);
            reader.ReadBoolean().Should().BeTrue(); // Name is not null
            reader.ReadString().Should().Be("John Doe");
            reader.ReadBoolean().Should().BeFalse(); // Email is null
        }

        [Test]
        public void Serialize_WhenMissingColumn_ShouldThrowArgumentException()
        {
            // Arrange
            var serializer = new RowSerializer();
            var tableInfo = new TableInfo("Users", new List<ColumnInfo> { new("Id", typeof(int)) }, 1);
            var row = new Dictionary<string, object>(); // Missing 'Id'

            // Act
            Action act = () => serializer.Serialize(tableInfo, row);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Missing value for column 'Id'");
        }
    }
}