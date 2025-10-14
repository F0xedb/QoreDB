namespace QoreDB.StorageEngine.Index.Interfaces
{
    /// <summary>
    /// Interface for serializing and deserializing B+ Tree nodes
    /// </summary>
    public interface INodeSerializer<TKey, TValue>
        where TKey : IComparable<TKey>
        where TValue : class
    {
        /// <summary>
        /// Serializes a node into a byte array for storage
        /// </summary>
        /// <param name="node">The node to serialize</param>
        /// <returns>A byte array representing the node</returns>
        void Serialize(INode<TKey> node, out byte[] pageDate);

        /// <summary>
        /// Deserializes a byte array back into a node object
        /// </summary>
        /// <param name="data">The byte array to deserialize</param>
        /// <returns>The deserialized node object</returns>
        INode<TKey> Deserialize(byte[] data);
    }
}
