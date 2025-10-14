namespace QoreDB.StorageEngine.Index.Interfaces
{
    /// <summary>
    /// Interface for a B+ Tree data structure.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the tree, which must be comparable.</typeparam>
    /// <typeparam name="TValue">The type of the values associated with the keys.</typeparam>
    public interface IBPlusTree<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        INode<TKey> Root { get; }

        /// <summary>
        /// Searches for a value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The value associated with the key, or the default value for the type if the key is not found.</returns>
        TValue? Search(TKey key);

        /// <summary>
        /// Inserts a key-value pair into the tree.
        /// </summary>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to associate with the key.</param>
        void Insert(TKey key, TValue value);

        /// <summary>
        /// Deletes a key-value pair from the tree based on the specified key.
        /// </summary>
        /// <param name="key">The key of the entry to delete.</param>
        void Delete(TKey key);

        /// <summary>
        /// Retrieves all values from the tree by traversing the leaf nodes
        /// </summary>
        /// <returns>An enumerable collection of all values in the tree</returns>
        IEnumerable<TValue> GetAllValues();
    }
}
