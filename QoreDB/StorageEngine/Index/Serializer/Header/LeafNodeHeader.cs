namespace QoreDB.StorageEngine.Index.Serializer.Header
{
    /// <summary>
    /// Represents the specific header for a Leaf Node page.
    /// </summary>
    /// <remarks>
    ///<para>
    /// On-Disk Format (Leaf Node):
    /// <code>
    /// |------------------------------------------------------------------------------------|
    /// | Header (LeafNodeHeader) |
    /// |------------------------------------------------------------------------------------|
    /// | Slot 0 Offset (2b) | Slot 1 Offset (2b) |... | Free Space... | Entry 1 Data | Entry 0 Data |
    /// |------------------------------------------------------------------------------------|
    /// </code>
    /// Entry Data Format: KeyLength (2b) | Key Bytes | ValueLength (2b) | Value Bytes
    /// </para>
    /// </remarks>
    public class LeafNodeHeader : NodeHeader
    {
        public int NextSiblingPageId { get; set; }
        public int PreviousSiblingPageId { get; set; }

        public LeafNodeHeader()
        {
            NodeType = LeafSerializationBytes.LEAF;
            Version = CurrentVersion;
        }

        public override int Size =>
            base.Size +
            sizeof(int) +   // NextSiblingPageId
            sizeof(int);    // PreviousSiblingPageId

        public override void WriteTo(BinaryWriter writer)
        {
            base.WriteTo(writer);
            writer.Write(NextSiblingPageId);
            writer.Write(PreviousSiblingPageId);
        }

        public override void ReadFrom(BinaryReader reader)
        {
            base.ReadFrom(reader);
            NextSiblingPageId = reader.ReadInt32();
            PreviousSiblingPageId = reader.ReadInt32();
        }
    }
}
