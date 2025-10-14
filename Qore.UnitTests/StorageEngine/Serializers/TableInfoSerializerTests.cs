using FluentAssertions;
using NUnit.Framework;
using QoreDB.Catalog.Models;
using QoreDB.StorageEngine.Index.Serializer;
using System;
using System.Collections.Generic;

namespace Qore.UnitTests.StorageEngine.Serializer
{
    [TestFixture]
    public class TableInfoSerializerTests
    {
        [Test]
        public void SerializeAndDeserialize_ShouldReturnEqualObject()
        {
            // Arrange
            var serializer = new TableInfoSerializer();
            var columns = new List<ColumnInfo>
            {
                new ColumnInfo("Id", typeof(int)),
                new ColumnInfo("Name", typeof(string))
            };
            var originalTable = new TableInfo("Users", columns, 42);

            // Act
            var serialized = serializer.Serialize(originalTable);
            var deserialized = serializer.Deserialize(serialized);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized.TableName.Should().Be(originalTable.TableName);
            deserialized.RootPage.Should().Be(originalTable.RootPage);
            deserialized.Columns.Should().HaveCount(2);
            deserialized.Columns[0].ColumnName.Should().Be("Id");
            deserialized.Columns[1].DataType.Should().Be(typeof(string));
        }
    }
}