namespace QoreDB.StorageEngine.Index.Serializer
{
    /// <summary>
    /// Contains the constant byte identifiers for node types.
    /// </summary>
    public class LeafSerializationBytes
    {
        /// <summary>
        /// The byte identifier for a Leaf Node.
        /// </summary>
        public const byte LEAF = 0x01;

        /// <summary>
        /// The byte identifier for an Internal Node.
        /// </summary>
        public const byte INTERNAL = 0x02;
    }
}
