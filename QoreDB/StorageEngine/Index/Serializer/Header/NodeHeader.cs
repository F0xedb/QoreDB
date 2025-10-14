namespace QoreDB.StorageEngine.Index.Serializer.Header
{
    /// <summary>
    /// Defines the common structure for all node page headers.
    /// </summary>
    public abstract class NodeHeader
    {
        /// <summary>
        /// The current version of the on-disk header format.
        /// This can be incremented if the header structure changes in the future.
        /// </summary>
        public const byte CurrentVersion = 1;

        /// <summary>
        /// Gets the type of the node (e.g., Leaf or Internal).
        /// </summary>
        public byte NodeType { get; protected set; }

        /// <summary>
        /// Gets or sets the version of the header format.
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// Gets or sets the page ID of this node's parent. A value of 0 indicates no parent (i.e., it's the root).
        /// </summary>
        public int ParentPageId { get; set; }

        /// <summary>
        /// Gets or sets the number of items (entries for a leaf, keys for an internal node) stored in this node.
        /// </summary>
        public ushort ItemCount { get; set; }

        /// <summary>
        /// Gets the total size of the header in bytes.
        /// </summary>
        public virtual int Size =>
            sizeof(byte) +  // NodeType
            sizeof(byte) +  // Version
            sizeof(int) +   // ParentPageId
            sizeof(ushort); // ItemCount (KeyCount for internal nodes)

        /// <summary>
        /// Writes the header's content to the beginning of a stream.
        /// </summary>
        public virtual void WriteTo(BinaryWriter writer)
        {
            writer.BaseStream.Position = 0;
            writer.Write(NodeType);
            writer.Write(Version);
            writer.Write(ParentPageId);
            writer.Write(ItemCount);
        }

        /// <summary>
        /// Reads the header's content from the beginning of a stream.
        /// </summary>
        public virtual void ReadFrom(BinaryReader reader)
        {
            reader.BaseStream.Position = 0;
            NodeType = reader.ReadByte();
            Version = reader.ReadByte();

            // In the future, you could check the version and handle older formats
            if (Version != CurrentVersion)
            {
                // throw new NotSupportedException($"Header version {Version} is not supported.");
            }

            ParentPageId = reader.ReadInt32();
            ItemCount = reader.ReadUInt16();
        }
    }
}
