namespace QoreDB.StorageEngine.Index.Interfaces
{
    /// <summary>
    /// Interface for serializing and deserializing keys and values
    /// for storage within the <see cref="IBPlusTree{TKey, TValue}"/> pages.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serializes an object into a byte array.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A byte array representing the object.</returns>
        byte[] Serialize(T obj);

        /// <summary>
        /// Deserializes a byte array back into an object.
        /// </summary>
        /// <param name="data">The byte array to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize(byte[] data);
    }
}
