using FluentAssertions;
using NUnit.Framework;
using QoreDB.Catalog.Models;
using QoreDB.StorageEngine.Index.Serializer;
using System;

namespace Qore.UnitTests.StorageEngine.Serializer
{
    [TestFixture]
    public class ColumnInfoSerializerTests
    {
        [Test]
        public void SerializeAndDeserialize_ShouldReturnEqualObject()
        {
            // Arrange
            var serializer = new ColumnInfoSerializer();
            var originalColumn = new ColumnInfo("UserId", typeof(int));

            // Act
            var serialized = serializer.Serialize(originalColumn);
            var deserialized = serializer.Deserialize(serialized);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized.ColumnName.Should().Be(originalColumn.ColumnName);
            deserialized.DataType.Should().Be(originalColumn.DataType);
        }
    }
}