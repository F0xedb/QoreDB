namespace QoreDB.StorageEngine.Index.Serializer.Header
{
    /// <summary>
    /// Represents the specific header for an Internal Node page.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On-Disk Format (Internal Node):
    /// <code>
    /// |------------------------------------------------------------------------------------|
    /// | Header (InternalNodeHeader) |
    /// |------------------------------------------------------------------------------------|
    /// | Slot 0 Offset (2b) | Slot 1 Offset (2b) |... | Free Space... | Pointer/Key 1 | Pointer/Key 0 |
    /// |------------------------------------------------------------------------------------|
    /// </code>
    /// Pointer/Key Data Format: KeyLength (2b) | Key Bytes | Right Child PageId (4b)
    /// </para>
    /// </remarks>
    public class InternalNodeHeader : NodeHeader
    {
        /// <summary>
        /// The page ID of the child to the left of all keys in the node.
        /// </summary>
        public int FirstChildPageId { get; set; }

        public InternalNodeHeader()
        {
            NodeType = LeafSerializationBytes.INTERNAL;
            Version = CurrentVersion;
        }

        public override int Size =>
            base.Size + // Default headers
            sizeof(int);    // FirstChildPageId

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(FirstChildPageId);
        }

        public override void ReadFrom(BinaryReader reader)
        {
            base.ReadFrom(reader);
            FirstChildPageId = reader.ReadInt32();
        }
    }
}
