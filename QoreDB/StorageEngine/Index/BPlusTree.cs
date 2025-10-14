using QoreDB.StorageEngine.Index.Interfaces;
using QoreDB.StorageEngine.Index.Models;
using QoreDB.StorageEngine.Index.Nodes;

namespace QoreDB.StorageEngine.Index
{
    /// <summary>
    /// A generic, in-memory implementation of a B+ Tree.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the tree.</typeparam>
    /// <typeparam name="TValue">The type of the values in the tree.</typeparam>
    public class BPlusTree<TKey, TValue> : IBPlusTree<TKey, TValue>
        where TKey : IComparable<TKey>
        where TValue : class
    {
        public INode<TKey> Root { get; protected set; }

        /// <summary>
        /// Gets the degree of the tree, which determines the maximum number of keys/children per node.
        /// A higher degree results in a shorter, wider tree, which is optimal for disk-based storage.
        /// </summary>
        protected readonly int Degree;

        /// <summary>
        /// Initializes a new instance of the <see cref="BPlusTree{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="degree">The degree of the tree. Must be at least 2.</param>
        /// <exception cref="ArgumentException">Thrown if the degree is less than 2.</exception>
        public BPlusTree(int RootPageId, int degree = Constants.DEFAULT_BTREE_PAGE_DEGREE)
        {
            if (degree < 2)
            {
                throw new ArgumentException("B+ Tree degree must be at least 2.", nameof(degree));
            }
            Degree = degree;

            Root = new LeafNode<TKey, TValue>
            {
                PageId = RootPageId
            };
        }

        public virtual TValue? Search(TKey key)
            => FindLeafNode(key).Entries
                .FirstOrDefault(e => e.Key.CompareTo(key) == 0)?.Value;

        public virtual void Insert(TKey key, TValue value)
        {
            var leaf = FindLeafNode(key);

            // Add the new entry to the leaf node in sorted order
            int insertIndex = leaf.FindIndex(key);
            leaf.Keys.Insert(insertIndex, key);
            leaf.Entries.Insert(insertIndex, new Entry<TKey, TValue> { Key = key, Value = value });

            // If the leaf node is overfull (number of keys >= degree), split it.
            if (leaf.Keys.Count >= Degree)
            {
                SplitNode(leaf);
            }
        }

        public virtual void Delete(TKey key)
        {
            var leaf = FindLeafNode(key);
            int index = leaf.FindIndex(key);

            // Check if the key actually exists in the leaf
            if (index < leaf.Keys.Count && leaf.Keys[index].CompareTo(key) == 0)
            {
                leaf.Keys.RemoveAt(index);
                leaf.Entries.RemoveAt(index);

                // A full implementation would now check for underflow (leaf.Keys.Count < _degree / 2)
                // and trigger redistribution from a sibling or merging with a sibling.
                // This logic is the reverse of the split operation and is omitted here.
            }
        }

        /// <summary>
        /// Traverses the tree from the root to find the appropriate leaf node for a given key.
        /// </summary>
        /// <param name="key">The key to find the leaf node for.</param>
        /// <returns>The leaf node where the key is or should be located.</returns>
        protected virtual LeafNode<TKey, TValue> FindLeafNode(TKey key)
        {
            INode<TKey> currentNode = Root;
            while (!currentNode.IsLeaf)
            {
                var internalNode = (InternalNode<TKey>)currentNode;
                int index = internalNode.FindIndex(key);

                // In an internal node, the key acts as a separator.
                // If the search key is >= the key at 'index', we need to follow the child pointer
                // to the right of that key, which is at 'index + 1'.
                if (index < internalNode.Keys.Count && key.CompareTo(internalNode.Keys[index]) >= 0)
                {
                    currentNode = internalNode.Children[index + 1];
                }
                else
                {
                    currentNode = internalNode.Children[index];
                }
            }
            return (LeafNode<TKey, TValue>)currentNode;
        }

        /// <summary>
        /// Splits a full node into two, promoting a key to the parent node.
        /// This is the fundamental mechanism by which the B+ Tree grows in height. [2]
        /// </summary>
        /// <param name="node">The node to split (can be a leaf or an internal node).</param>
        public virtual void SplitNode(Node<TKey> node)
        {
            int midIndex = node.Keys.Count / 2;
            TKey keyToPromote;
            INode<TKey> newSibling;

            if (node.IsLeaf)
            {
                var leaf = (LeafNode<TKey, TValue>)node;
                var newLeaf = new LeafNode<TKey, TValue>();
                keyToPromote = leaf.Keys[midIndex];

                // Move the second half of entries to the new leaf
                newLeaf.Keys.AddRange(leaf.Keys.Skip(midIndex));
                newLeaf.Entries.AddRange(leaf.Entries.Skip(midIndex));
                leaf.Keys.RemoveRange(midIndex, leaf.Keys.Count - midIndex);
                leaf.Entries.RemoveRange(midIndex, leaf.Entries.Count - midIndex);

                // Update the doubly-linked list pointers
                newLeaf.NextSibling = leaf.NextSibling;
                if (newLeaf.NextSibling != null) newLeaf.NextSibling.PreviousSibling = newLeaf;
                leaf.NextSibling = newLeaf;
                newLeaf.PreviousSibling = leaf;

                newSibling = newLeaf;
            }
            else // It's an Internal Node
            {
                var internalNode = (InternalNode<TKey>)node;
                var newInternal = new InternalNode<TKey>();
                keyToPromote = internalNode.Keys[midIndex];

                // For internal nodes, the promoted key is removed from the node keys list
                internalNode.Keys.RemoveAt(midIndex);

                // Move keys and children to the new sibling
                newInternal.Keys.AddRange(internalNode.Keys.Skip(midIndex));
                newInternal.Children.AddRange(internalNode.Children.Skip(midIndex + 1));
                internalNode.Keys.RemoveRange(midIndex, internalNode.Keys.Count - midIndex);
                internalNode.Children.RemoveRange(midIndex + 1, internalNode.Children.Count - (midIndex + 1));

                // Update parent pointers for the moved children
                foreach (var child in newInternal.Children)
                {
                    child.Parent = newInternal;
                }
                newSibling = newInternal;
            }

            // --- Promote the middle key to the parent node ---
            if (node.IsRoot)
            {
                // If the root splits, create a new root, increasing the tree's height by one.
                var newRoot = new InternalNode<TKey>();
                newRoot.Keys.Add(keyToPromote);
                newRoot.Children.Add(node);
                newRoot.Children.Add(newSibling);
                Root = newRoot;
                node.Parent = newRoot;
                newSibling.Parent = newRoot;
            }
            else
            {
                var parent = (InternalNode<TKey>)node.Parent;
                int insertIndex = parent.FindIndex(keyToPromote);
                parent.Keys.Insert(insertIndex, keyToPromote);
                parent.Children.Insert(insertIndex + 1, newSibling);
                newSibling.Parent = parent;

                // Recursively check if the parent now needs to be split
                if (parent.Keys.Count >= Degree)
                {
                    SplitNode(parent);
                }
            }
        }
        
        /// <summary>
        /// Finds the very first leaf node in the tree
        /// </summary>
        protected virtual LeafNode<TKey, TValue> FindFirstLeafNode()
        {
            INode<TKey> currentNode = Root;
            while (!currentNode.IsLeaf)
            {
                var internalNode = (InternalNode<TKey>)currentNode;
                currentNode = internalNode.Children[0];
            }
            return (LeafNode<TKey, TValue>)currentNode;
        }

        public virtual IEnumerable<TValue> GetAllValues()
        {
            var currentLeaf = FindFirstLeafNode();
            while (currentLeaf != null)
            {
                foreach (var entry in currentLeaf.Entries)
                {
                    yield return entry.Value;
                }
                currentLeaf = (LeafNode<TKey, TValue>)currentLeaf.NextSibling;
            }
        }
    }
}
