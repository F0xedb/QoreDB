using QoreDB.StorageEngine.Index.Interfaces;

namespace QoreDB.StorageEngine.Index.Nodes
{
    /// <summary>
    /// Provides an abstract base implementation for all nodes in the B+ Tree.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the node, which must be comparable.</typeparam>
    public abstract class Node<TKey> : INode<TKey>
        where TKey : IComparable<TKey>
    {
        public List<TKey> Keys { get; set; }

        public INode<TKey> Parent { get; set; }

        public int PageId { get; set; }

        public int ParentPageId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node{TKey}"/> class.
        /// </summary>
        protected Node()
        {
            Keys = new List<TKey>();
        }

        public bool IsRoot => Parent == null;

        public virtual IEnumerable<INode<TKey>> GetChildren()
            => Enumerable.Empty<INode<TKey>>();


        public abstract bool IsLeaf { get; }

        /// <summary>
        /// Performs a binary search to find the index of a given key or the index where it should be inserted.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>The zero-based index of the key in the list, or a negative number that is the bitwise complement of the index of the next element that is larger than the key.</returns>
        public int FindIndex(TKey key)
        {
            int index = Keys.BinarySearch(key);
            return index < 0 ? ~index : index;
        }
    }
}
