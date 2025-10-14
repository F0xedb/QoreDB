namespace QoreDB.StorageEngine.Index.Models
{
    /// <summary>
    /// Represents a single key-value pair stored within a leaf node of the B+ Tree.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class Entry<TKey, TValue>
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public TValue Value { get; set; }
    }
}
