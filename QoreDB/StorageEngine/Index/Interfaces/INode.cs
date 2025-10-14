namespace QoreDB.StorageEngine.Index.Interfaces
{
    /// <summary>
    /// Defines the contract for a node within a B+ Tree.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys stored in the node.</typeparam>
    public interface INode<TKey>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Gets the list of keys stored in the node.
        /// </summary>
        List<TKey> Keys { get; }

        /// <summary>
        /// Gets or sets the parent of this node.
        /// </summary>
        INode<TKey> Parent { get; set; }

        /// <summary>
        /// The ID of the page where this node is stored.
        /// </summary>
        public int PageId { get; set; }

        /// <summary>
        /// The Page ID of this node's parent. Populated during deserialization.
        /// </summary>
        public int ParentPageId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this node is a leaf node.
        /// </summary>
        bool IsLeaf { get; }

        /// <summary>
        /// Gets a value indicating whether this node is the root of the tree.
        /// </summary>
        bool IsRoot { get; }

        /// <summary>
        /// Gets the child nodes of this node (for internal nodes)
        /// </summary>
        IEnumerable<INode<TKey>> GetChildren();
    }
}
