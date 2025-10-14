using FluentAssertions;
using QoreDB.StorageEngine;
using QoreDB.StorageEngine.Index.Serializer;
using QoreDB.StorageEngine.Index.Serializer.Header;

namespace Qore.UnitTests.StorageEngine
{
    [TestFixture]
    public class NodeHeaderTests
    {
        [Test]
        public void LeafNodeHeader_Size_ShouldBeCorrect()
        {
            var header = new LeafNodeHeader();
            // 1 (Type) + 1 (Version) + 4 (Parent) + 2 (Count) + 4 (Next) + 4 (Prev) = 16 bytes
            header.Size.Should().Be(16);
        }

        [Test]
        public void LeafNodeHeader_Serialization_ShouldRoundtripCorrectly()
        {
            // Arrange
            var originalHeader = new LeafNodeHeader
            {
                ParentPageId = 123,
                ItemCount = 42,
                NextSiblingPageId = 456,
                PreviousSiblingPageId = 789
            };
            var buffer = new byte[Constants.DEFAULT_PAGE_SIZE];

            // Act
            using (var writer = new BinaryWriter(new MemoryStream(buffer)))
            {
                originalHeader.WriteTo(writer);
            }

            var newHeader = new LeafNodeHeader();
            using (var reader = new BinaryReader(new MemoryStream(buffer)))
            {
                newHeader.ReadFrom(reader);
            }

            // Assert
            newHeader.NodeType.Should().Be(LeafSerializationBytes.LEAF);
            newHeader.Version.Should().Be(NodeHeader.CurrentVersion);
            newHeader.ParentPageId.Should().Be(originalHeader.ParentPageId);
            newHeader.ItemCount.Should().Be(originalHeader.ItemCount);
            newHeader.NextSiblingPageId.Should().Be(originalHeader.NextSiblingPageId);
            newHeader.PreviousSiblingPageId.Should().Be(originalHeader.PreviousSiblingPageId);
        }

        [Test]
        public void InternalNodeHeader_Size_ShouldBeCorrect()
        {
            var header = new InternalNodeHeader();
            // 1 (Type) + 1 (Version) + 4 (Parent) + 2 (Count) + 4 (FirstChild) = 12 bytes
            header.Size.Should().Be(12);
        }

        [Test]
        public void InternalNodeHeader_Serialization_ShouldRoundtripCorrectly()
        {
            // Arrange
            var originalHeader = new InternalNodeHeader
            {
                ParentPageId = 987,
                ItemCount = 21,
                FirstChildPageId = 654
            };
            var buffer = new byte[Constants.DEFAULT_PAGE_SIZE];

            // Act
            using (var writer = new BinaryWriter(new MemoryStream(buffer)))
            {
                originalHeader.WriteTo(writer);
            }

            var newHeader = new InternalNodeHeader();
            using (var reader = new BinaryReader(new MemoryStream(buffer)))
            {
                newHeader.ReadFrom(reader);
            }

            // Assert
            newHeader.NodeType.Should().Be(LeafSerializationBytes.INTERNAL);
            newHeader.Version.Should().Be(NodeHeader.CurrentVersion);
            newHeader.ParentPageId.Should().Be(originalHeader.ParentPageId);
            newHeader.ItemCount.Should().Be(originalHeader.ItemCount);
            newHeader.FirstChildPageId.Should().Be(originalHeader.FirstChildPageId);
        }
    }
}
