using QoreDB.StorageEngine.Index.Models;

namespace QoreDB.StorageEngine.Index.Nodes
{
    /// <summary>
    /// Represents a leaf node in the B+ Tree. Leaf nodes store the actual key-value pairs (entries).
    /// All leaf nodes are linked together in a doubly-linked list for efficient range scans.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the node.</typeparam>
    /// <typeparam name="TValue">The type of the values in the node.</typeparam>
    public class LeafNode<TKey, TValue> : Node<TKey>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Gets or sets the list of key-value entries stored in this leaf.
        /// </summary>
        public List<Entry<TKey, TValue>> Entries { get; set; }

        /// <summary>
        /// Gets or sets the next leaf node in the sequence, forming a linked list.
        /// </summary>
        public LeafNode<TKey, TValue> NextSibling { get; set; }

        /// <summary>
        /// Gets or sets the previous leaf node in the sequence, forming a doubly-linked list.
        /// </summary>
        public LeafNode<TKey, TValue> PreviousSibling { get; set; }

        /// <summary>
        /// The Page ID of the next sibling. Populated during deserialization.
        /// </summary>
        public int NextSiblingPageId { get; set; }

        /// <summary>
        /// The Page ID of the previous sibling. Populated during deserialization.
        /// </summary>
        public int PreviousSiblingPageId { get; set; }

        public override bool IsLeaf => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeafNode{TKey, TValue}"/> class.
        /// </summary>
        public LeafNode() : base()
        {
            Entries = new List<Entry<TKey, TValue>>();
        }
    }
}
