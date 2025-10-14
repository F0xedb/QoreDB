using QoreDB.StorageEngine.Index.Interfaces;

namespace QoreDB.StorageEngine.Index.Nodes
{
    /// <summary>
    /// Represents an internal (non-leaf) node in the B+ Tree. Internal nodes contain keys
    /// that act as routers and pointers to child nodes. They do not store values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the node.</typeparam>
    public class InternalNode<TKey> : Node<TKey>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// Gets or sets the list of child nodes. An internal node with 'k' keys has 'k+1' children.
        /// </summary>
        public List<INode<TKey>> Children { get; set; }

        public override IEnumerable<INode<TKey>> GetChildren()
            => Children;

        /// <summary>
        /// The Page IDs of the child nodes. Populated during deserialization.
        /// The actual Node objects are loaded lazily by the TreeManager.
        /// </summary>
        public List<int> ChildrenPageIds { get; set; } = [];
        public override bool IsLeaf => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalNode{TKey}"/> class.
        /// </summary>
        public InternalNode() : base()
        {
            Children = new List<INode<TKey>>();
        }

        public void AddChild(INode<TKey> child)
        {
            Children.Add(child);
            ChildrenPageIds.Add(child.PageId);
        }
    }
}
