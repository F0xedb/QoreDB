using FluentAssertions;
using QoreDB.StorageEngine.Index.Models;
using QoreDB.StorageEngine.Index.Nodes;
using QoreDB.StorageEngine.Index.Serializer;
using QoreDB.StorageEngine.Index.Serializer.Header;
using QoreDB.StorageEngine.Index.Serializer.Implementations;
using System.Text;

namespace Qore.UnitTests.StorageEngine
{
    [TestFixture]
    public class NodeSerializerTests
    {
        private const int PageSize = 256; // Small page size for easier testing
        private NodeSerializer<int, string> _serializer;

        [SetUp]
        public void Setup()
        {
            _serializer = new NodeSerializer<int, string>(new IntSerializer(), new StringSerializer(), PageSize);
        }

        [Test]
        public void SerializeAndDeserialize_LeafNodeWithData_ShouldRoundtripCorrectly()
        {
            // Arrange
            var originalNode = new LeafNode<int, string>() { ParentPageId = 1, NextSiblingPageId = 3, PreviousSiblingPageId = 0 };
            originalNode.Entries.Add(new Entry<int, string> { Key = 10, Value = "Ten" });
            originalNode.Entries.Add(new Entry<int, string> { Key = 20, Value = "Twenty" });
            originalNode.Keys.AddRange([10, 20]);

            // Act
            _serializer.Serialize(originalNode, out var pageData);
            var deserializedNode = (LeafNode<int, string>)_serializer.Deserialize(pageData);

            // Assert
            deserializedNode.Should().NotBeNull();
            deserializedNode.IsLeaf.Should().BeTrue();
            deserializedNode.ParentPageId.Should().Be(1);
            deserializedNode.NextSiblingPageId.Should().Be(3);
            deserializedNode.PreviousSiblingPageId.Should().Be(0);
            deserializedNode.Entries.Count.Should().Be(2);
            deserializedNode.Keys.Should().Equal(10, 20);
            deserializedNode.Entries.Select(e => e.Value).Should().Equal("Ten", "Twenty");
        }

        [Test]
        public void SerializeAndDeserialize_InternalNodeWithData_ShouldRoundtripCorrectly()
        {
            // Arrange
            var originalNode = new InternalNode<int> { ParentPageId = 1 };
            originalNode.Keys.AddRange([50, 100]);
            originalNode.AddChild(new LeafNode<int, string>() { PageId = 10 });
            originalNode.AddChild(new LeafNode<int, string>() { PageId = 20 });
            originalNode.AddChild(new LeafNode<int, string>() { PageId = 30 });

            // Act
            _serializer.Serialize(originalNode, out var pageData);
            var deserializedNode = (InternalNode<int>)_serializer.Deserialize(pageData);

            // Assert
            deserializedNode.Should().NotBeNull();
            deserializedNode.IsLeaf.Should().BeFalse();
            deserializedNode.ParentPageId.Should().Be(1);
            deserializedNode.Keys.Count.Should().Be(2);
            deserializedNode.ChildrenPageIds.Count.Should().Be(3);
            deserializedNode.Keys.Should().Equal(50, 100);
            deserializedNode.ChildrenPageIds.Should().Equal(10, 20, 30);
        }

        [Test]
        public void Deserialize_UninitializedPage_ReturnsEmptyLeafNode()
        {
            // Arrange
            var emptyPage = new byte[PageSize]; // Already initialized to zeros

            // Act
            var node = _serializer.Deserialize(emptyPage);

            // Assert
            node.Should().NotBeNull();
            node.IsLeaf.Should().BeTrue();
            node.Keys.Should().BeEmpty();
            (node as LeafNode<int, string>)?.Entries.Should().BeEmpty();
        }

        [Test]
        public void Serialize_LeafNode_ShouldHaveCorrectByteLayout()
        {
            // Arrange
            var node = new LeafNode<int, string>() { ParentPageId = 123, NextSiblingPageId = 456, PreviousSiblingPageId = 789 };
            node.Entries.Add(new Entry<int, string> { Key = 1, Value = "A" }); // Key(4b)+Val(1b)+Lens(4b) = 9 bytes
            node.Entries.Add(new Entry<int, string> { Key = 2, Value = "BB" });// Key(4b)+Val(2b)+Lens(4b) = 10 bytes
            node.Keys.AddRange([1, 2]);

            // Act
            _serializer.Serialize(node, out var pageData);

            // Assert Header
            pageData[0].Should().Be(LeafSerializationBytes.LEAF); // Type
            BitConverter.ToInt32(pageData, 2).Should().Be(123); // ParentPageId (offset includes version byte)
            BitConverter.ToUInt16(pageData, 6).Should().Be(2);  // ItemCount (offset includes version+parent)
            BitConverter.ToInt32(pageData, 8).Should().Be(456); // NextSibling
            BitConverter.ToInt32(pageData, 12).Should().Be(789);// PrevSibling

            // Assert Slots (right after header)
            var headerSize = new LeafNodeHeader().Size;
            var slot0Offset = BitConverter.ToUInt16(pageData, headerSize);
            var slot1Offset = BitConverter.ToUInt16(pageData, headerSize + sizeof(ushort)); // shift over one slot after the header

            // Assert Data (at the end of the page)
            // Entry 1 ("BB") is written first at the end
            int entry1Start = PageSize - 10;
            slot1Offset.Should().Be((ushort)entry1Start);
            BitConverter.ToUInt16(pageData, entry1Start).Should().Be(4); // Key length
            BitConverter.ToInt32(pageData, entry1Start + 2).Should().Be(2); // Key
            BitConverter.ToUInt16(pageData, entry1Start + 6).Should().Be(2); // Value length
            Encoding.UTF8.GetString(pageData, entry1Start + 8, 2).Should().Be("BB"); // Value

            // Entry 0 ("A") is written before it
            int entry0Start = entry1Start - 9;
            slot0Offset.Should().Be((ushort)entry0Start);
            BitConverter.ToUInt16(pageData, entry0Start).Should().Be(4); // Key length
            BitConverter.ToInt32(pageData, entry0Start + 2).Should().Be(1); // Key
            BitConverter.ToUInt16(pageData, entry0Start + 6).Should().Be(1); // Value length
            Encoding.UTF8.GetString(pageData, entry0Start + 8, 1).Should().Be("A"); // Value
        }
    }
}
