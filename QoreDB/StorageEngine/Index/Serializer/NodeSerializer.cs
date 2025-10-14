using QoreDB.StorageEngine.Index.Interfaces;
using QoreDB.StorageEngine.Index.Models;
using QoreDB.StorageEngine.Index.Nodes;
using QoreDB.StorageEngine.Index.Serializer.Header;

namespace QoreDB.StorageEngine.Index.Serializer
{
    /// <summary>
    /// Handles the serialization and deserialization of B+ Tree nodes to and from byte arrays.
    /// </summary>
    public class NodeSerializer<TKey, TValue> : INodeSerializer<TKey, TValue>
        where TKey : IComparable<TKey>
        where TValue : class
    {
        private readonly ISerializer<TKey> _keySerializer;
        private readonly ISerializer<TValue> _valueSerializer;
        private int _pageSize;

        public NodeSerializer(ISerializer<TKey> keySerializer, ISerializer<TValue> valueSerializer, int pageSize = Constants.DEFAULT_PAGE_SIZE)
        {
            _keySerializer = keySerializer;
            _valueSerializer = valueSerializer;
            _pageSize = pageSize;
        }

        /// <summary>
        /// Serializes a node into a byte array.
        /// </summary>
        /// <param name="node">The node to serialize.</param>
        /// <param name="pageData">The target byte array to write into.</param>
        /// <param name="parentPageId">The page ID of the node's parent.</param>
        /// <param name="nextSiblingPageId">The page ID of the node's next sibling (for leaves).</param>
        /// <param name="prevSiblingPageId">The page ID of the node's previous sibling (for leaves).</param>
        public void Serialize(INode<TKey> node, out byte[] pageData)
        {
            // Clear the page data to ensure we don't have leftover bytes from previous state.
            pageData = new byte[_pageSize];

            if (node.IsLeaf && node is LeafNode<TKey, TValue> leaf)
                SerializeLeaf(leaf, pageData, leaf.ParentPageId, leaf.NextSiblingPageId, leaf.PreviousSiblingPageId);
            else if (!node.IsLeaf && node is InternalNode<TKey> internalNode)
                SerializeInternal(internalNode, pageData, node.ParentPageId);
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Deserializes a byte array into a Node object.
        /// </summary>
        /// <param name="pageData">The source byte array to read from.</param>
        /// <returns>A deserialized LeafNode or InternalNode.</returns>
        public INode<TKey> Deserialize(byte[] pageData)
        {
            // Peek at the first byte to determine the node type.
            var nodeType = pageData[0];
            return nodeType switch
            {
                LeafSerializationBytes.INTERNAL => DeserializeInternal(pageData),
                LeafSerializationBytes.LEAF => DeserializeLeaf(pageData),
                _ => new LeafNode<TKey, TValue>() // Return an empty node for an uninitialized page
            };
        }

        /// <summary>
        /// Serializes a leaf node into a byte array using a slotted page format.
        /// </summary>
        /// <param name="node">The leaf node to serialize.</param>
        /// <param name="pageData">The target byte array to write into.</param>
        /// <param name="parentPageId">The page ID of the node's parent.</param>
        /// <param name="nextSiblingId">The page ID of the node's next sibling.</param>
        /// <param name="prevSiblingId">The page ID of the node's previous sibling.</param>
        private void SerializeLeaf(LeafNode<TKey, TValue> node, byte[] pageData, int parentPageId, int nextSiblingId, int prevSiblingId)
        {
            using var writer = new BinaryWriter(new MemoryStream(pageData));

            // 1. Write Header
            var header = new LeafNodeHeader
            {
                ParentPageId = parentPageId,
                ItemCount = (ushort)node.Entries.Count,
                NextSiblingPageId = nextSiblingId,
                PreviousSiblingPageId = prevSiblingId
            };

            header.WriteTo(writer);

            // 2. Write Entry Data from the end of the page backwards
            int currentOffset = pageData.Length;
            var slotOffsets = new ushort[node.Entries.Count];

            for (int i = node.Entries.Count - 1; i >= 0; i--)
            {
                var entry = node.Entries[i];
                var keyBytes = _keySerializer.Serialize(entry.Key);
                var valueBytes = _valueSerializer.Serialize(entry.Value);

                currentOffset -= valueBytes.Length;
                writer.BaseStream.Position = currentOffset;
                writer.Write(valueBytes);

                currentOffset -= sizeof(ushort);
                writer.BaseStream.Position = currentOffset;
                writer.Write((ushort)valueBytes.Length);

                currentOffset -= keyBytes.Length;
                writer.BaseStream.Position = currentOffset;
                writer.Write(keyBytes);

                currentOffset -= sizeof(ushort);
                writer.BaseStream.Position = currentOffset;
                writer.Write((ushort)keyBytes.Length);

                slotOffsets[i] = (ushort)currentOffset;
            }

            // 3. Write the slot offsets after the header
            writer.BaseStream.Position = header.Size;
            foreach (var offset in slotOffsets)
            {
                writer.Write(offset);
            }
        }

        /// <summary>
        /// Deserializes a byte array representing a leaf node back into a <see cref="LeafNode{TKey, TValue}"/> object.
        /// This method reads the slotted page format written by <see cref="SerializeLeaf"/>.
        /// </summary>
        /// <param name="pageData">The source byte array to read from.</param>
        /// <returns>A new, populated <see cref="LeafNode{TKey, TValue}"/> instance.</returns>
        private LeafNode<TKey, TValue> DeserializeLeaf(byte[] pageData)
        {
            var node = new LeafNode<TKey, TValue>();
            using var reader = new BinaryReader(new MemoryStream(pageData));

            // 1. Read Header
            var header = new LeafNodeHeader();
            header.ReadFrom(reader);

            node.ParentPageId = header.ParentPageId;
            node.NextSiblingPageId = header.NextSiblingPageId;
            node.PreviousSiblingPageId = header.PreviousSiblingPageId;
            ushort entryCount = header.ItemCount;

            // 2. Read Slot Offsets
            var slotOffsets = new ushort[entryCount];
            for (var i = 0; i < entryCount; i++)
            {
                slotOffsets[i] = reader.ReadUInt16();
            }

            // 3. Read Entry Data using offsets
            for (var i = 0; i < entryCount; i++)
            {
                reader.BaseStream.Position = slotOffsets[i];

                var keyLength = reader.ReadUInt16();
                var keyBytes = reader.ReadBytes(keyLength);
                TKey key = _keySerializer.Deserialize(keyBytes);

                var valueLength = reader.ReadUInt16();
                var valueBytes = reader.ReadBytes(valueLength);
                TValue value = _valueSerializer.Deserialize(valueBytes);

                node.Keys.Add(key);
                node.Entries.Add(new Entry<TKey, TValue> { Key = key, Value = value });
            }

            return node;
        }

        /// <summary>
        /// Serializes an internal node into a byte array using a slotted page format.
        /// </summary>
        /// <param name="node">The internal node to serialize.</param>
        /// <param name="pageData">The target byte array to write into.</param>
        /// <param name="parentPageId">The page ID of the node's parent.</param>
        private void SerializeInternal(InternalNode<TKey> node, byte[] pageData, int parentPageId)
        {
            using var writer = new BinaryWriter(new MemoryStream(pageData));

            // 1. Write Header
            var header = new InternalNodeHeader
            {
                ParentPageId = parentPageId,
                ItemCount = (ushort)node.Keys.Count,
                FirstChildPageId = node.ChildrenPageIds.FirstOrDefault()
            };

            header.WriteTo(writer);

            // 2. Write Key/Pointer Data from the end of the page backwards
            var currentOffset = pageData.Length;
            var slotOffsets = new ushort[node.Keys.Count];

            for (var i = node.Keys.Count - 1; i >= 0; i--)
            {
                var keyBytes = _keySerializer.Serialize(node.Keys[i]);
                var childPageId = node.ChildrenPageIds[i + 1]; // The pointer to the right of the key

                currentOffset -= sizeof(int);
                writer.BaseStream.Position = currentOffset;
                writer.Write(childPageId);

                currentOffset -= keyBytes.Length;
                writer.BaseStream.Position = currentOffset;
                writer.Write(keyBytes);

                currentOffset -= sizeof(ushort);
                writer.BaseStream.Position = currentOffset;
                writer.Write((ushort)keyBytes.Length);

                slotOffsets[i] = (ushort)currentOffset;
            }

            // 3. Write the slot offsets after the header
            writer.BaseStream.Position = header.Size;

            foreach (var offset in slotOffsets)
            {
                writer.Write(offset);
            }
        }

        /// <summary>
        /// Deserializes a byte array representing an internal node back into an <see cref="InternalNode{TKey}"/> object.
        /// This method reads the slotted page format written by <see cref="SerializeInternal"/>.
        /// </summary>
        /// <param name="pageData">The source byte array to read from.</param>
        /// <returns>A new, populated <see cref="InternalNode{TKey}"/> instance.</returns>
        private InternalNode<TKey> DeserializeInternal(byte[] pageData)
        {
            var node = new InternalNode<TKey>();
            using var reader = new BinaryReader(new MemoryStream(pageData));

            // 1. Read Header
            var header = new InternalNodeHeader();
            header.ReadFrom(reader);

            node.ParentPageId = header.ParentPageId;
            ushort keyCount = header.ItemCount;
            (node.ChildrenPageIds as List<int>).Add(header.FirstChildPageId);

            // 2. Read Slot Offsets
            var slotOffsets = new ushort[keyCount];
            for (var i = 0; i < keyCount; i++)
            {
                slotOffsets[i] = reader.ReadUInt16();
            }

            // 3. Read Key/Pointer Data using offsets
            for (var i = 0; i < keyCount; i++)
            {
                reader.BaseStream.Position = slotOffsets[i];

                var keyLength = reader.ReadUInt16();
                var keyBytes = reader.ReadBytes(keyLength);
                TKey key = _keySerializer.Deserialize(keyBytes);
                var childPageId = reader.ReadInt32();

                node.Keys.Add(key);
                (node.ChildrenPageIds as List<int>).Add(childPageId);
            }

            return node;
        }
    }
}
