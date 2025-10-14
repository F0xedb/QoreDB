using QoreDB.Catalog;
using QoreDB.Catalog.Interfaces;
using QoreDB.StorageEngine.Index.Interfaces;
using QoreDB.StorageEngine.Index.Models;
using QoreDB.StorageEngine.Index.Nodes;
using QoreDB.StorageEngine.Pager.Interfaces;

namespace QoreDB.StorageEngine.Index
{
    /// <summary>
    /// A BPlusTree implementation that persists its nodes to a pager,
    /// extending the in-memory BPlusTree logic
    /// </summary>
    public class BackingStorageBPlusTree<TKey, TValue> : BPlusTree<TKey, TValue>
        where TKey : IComparable<TKey>
        where TValue : class
    {
        private readonly IPager _pager;
        private readonly INodeSerializer<TKey, TValue> _serializer;

        /// <summary>
        /// The page number of the root node
        /// This is the single pointer we need to store to find the tree
        /// </summary>
        public int RootPageNumber { get; private set; }

        public string TableName { get; }

        public ICatalogManager CatalogManager { get; }

        public BackingStorageBPlusTree(
            int rootPageNumber,
            int degree,
            IPager pager,
            INodeSerializer<TKey, TValue> serializer,
            ICatalogManager catalogManager,
            string tableName
        )
            : base(rootPageNumber, degree)
        {
            _pager = pager;
            _serializer = serializer;
            CatalogManager = catalogManager;
            RootPageNumber = rootPageNumber;
            TableName = tableName;

            if (_pager.PageExists(RootPageNumber))
            {
                Root = ReadNode(RootPageNumber);
            }
            else
            {
                var newPageId = _pager.AllocatePage();
                Root.PageId = newPageId;
                SetRootPageNumber(Root.PageId);
                WriteNode(Root);
            }
        }

        // ... Search and Insert methods are unchanged ...
        public override TValue? Search(TKey key)
        {
            var leaf = FindLeafNode(key);
            return leaf.Entries.FirstOrDefault(e => e.Key.CompareTo(key) == 0)?.Value;
        }

        public override void Insert(TKey key, TValue value)
        {
            var leaf = FindLeafNode(key);

            int insertIndex = leaf.FindIndex(key);
            leaf.Keys.Insert(insertIndex, key);
            leaf.Entries.Insert(insertIndex, new Entry<TKey, TValue> { Key = key, Value = value });

            if (leaf.Keys.Count >= Degree)
            {
                SplitNode(leaf);
            }
            else
            {
                WriteNode(leaf);
            }
        }

        public override void SplitNode(Node<TKey> node)
        {
            int midIndex = node.Keys.Count / 2;
            TKey keyToPromote;
            INode<TKey> newSibling;

            var newSiblingPageId = _pager.AllocatePage();

            if (node.IsLeaf)
            {
                var leaf = (LeafNode<TKey, TValue>)node;
                var newLeaf = new LeafNode<TKey, TValue> { PageId = newSiblingPageId };
                keyToPromote = leaf.Keys[midIndex];

                newLeaf.Keys.AddRange(leaf.Keys.Skip(midIndex));
                newLeaf.Entries.AddRange(leaf.Entries.Skip(midIndex));

                leaf.Keys.RemoveRange(midIndex, leaf.Keys.Count - midIndex);
                leaf.Entries.RemoveRange(midIndex, leaf.Entries.Count - midIndex);

                newLeaf.NextSiblingPageId = leaf.NextSiblingPageId;
                leaf.NextSiblingPageId = newLeaf.PageId;
                newLeaf.PreviousSiblingPageId = leaf.PageId;

                if (newLeaf.NextSiblingPageId > 0)
                {
                    var nextSiblingNode = (LeafNode<TKey, TValue>)ReadNode(newLeaf.NextSiblingPageId);
                    nextSiblingNode.PreviousSiblingPageId = newLeaf.PageId;
                    WriteNode(nextSiblingNode);
                }

                newSibling = newLeaf;
            }
            else // It's an Internal Node
            {
                var internalNode = (InternalNode<TKey>)node;
                var newInternal = new InternalNode<TKey> { PageId = newSiblingPageId };
                keyToPromote = internalNode.Keys[midIndex];
                internalNode.Keys.RemoveAt(midIndex);

                newInternal.Keys.AddRange(internalNode.Keys.Skip(midIndex));
                internalNode.Keys.RemoveRange(midIndex, internalNode.Keys.Count - midIndex);

                var internalChildren = (List<int>)internalNode.ChildrenPageIds;
                var newInternalChildren = (List<int>)newInternal.ChildrenPageIds;

                newInternalChildren.AddRange(internalChildren.Skip(midIndex + 1));
                internalChildren.RemoveRange(midIndex + 1, internalChildren.Count - (midIndex + 1));

                newSibling = newInternal;

                foreach (var childId in newInternal.ChildrenPageIds)
                {
                    var childNode = ReadNode(childId);
                    childNode.Parent = newInternal;
                    WriteNode(childNode);
                }
            }

            WriteNode(node);
            WriteNode(newSibling);

            if (node.IsRoot)
            {
                var newRootPageId = _pager.AllocatePage();
                var newRoot = new InternalNode<TKey> { PageId = newRootPageId };
                newRoot.Keys.Add(keyToPromote);

                newRoot.AddChild(node);
                newRoot.AddChild(newSibling);

                Root = newRoot;
                SetRootPageNumber(newRoot.PageId);
                node.Parent = newRoot;
                newSibling.Parent = newRoot;

                WriteNode(newRoot);
                WriteNode(node);
                WriteNode(newSibling);
            }
            else
            {
                var parent = (InternalNode<TKey>)ReadNode(node.Parent.PageId);
                int insertIndex = parent.FindIndex(keyToPromote);
                parent.Keys.Insert(insertIndex, keyToPromote);
                parent.ChildrenPageIds.Insert(insertIndex + 1, newSiblingPageId);

                newSibling.Parent = parent;

                if (parent.Keys.Count >= Degree)
                {
                    SplitNode(parent);
                }
                else
                {
                    WriteNode(parent);
                }
            }
        }

        protected override LeafNode<TKey, TValue> FindFirstLeafNode()
        {
            INode<TKey> currentNode = ReadNode(Root.PageId);
            while (!currentNode.IsLeaf)
            {
                var internalNode = (InternalNode<TKey>)currentNode;
                var childNode = ReadNode(internalNode.ChildrenPageIds[0]);
                childNode.Parent = internalNode;
                currentNode = childNode;
            }
            return (LeafNode<TKey, TValue>)currentNode;
        }

        public override IEnumerable<TValue> GetAllValues()
        {
            var currentLeaf = FindFirstLeafNode();
            while (currentLeaf != null)
            {
                foreach (var entry in currentLeaf.Entries)
                {
                    yield return entry.Value;
                }

                // A PageId of 0 is invalid, so we check for > 0.
                if (currentLeaf.NextSiblingPageId > 0)
                {
                    currentLeaf = (LeafNode<TKey, TValue>)ReadNode(currentLeaf.NextSiblingPageId);
                }
                else
                {
                    currentLeaf = null; // End of the list
                }
            }
        }

        private new LeafNode<TKey, TValue> FindLeafNode(TKey key)
        {
            INode<TKey> currentNode = ReadNode(Root.PageId);

            while (!currentNode.IsLeaf)
            {
                var internalNode = (InternalNode<TKey>)currentNode;
                int index = internalNode.FindIndex(key);

                int nextPageId;
                if (index < internalNode.Keys.Count && key.CompareTo(internalNode.Keys[index]) >= 0)
                {
                    nextPageId = internalNode.ChildrenPageIds[index + 1];
                }
                else
                {
                    nextPageId = internalNode.ChildrenPageIds[index];
                }

                currentNode = ReadNode(nextPageId);
                currentNode.Parent = internalNode;
            }
            return (LeafNode<TKey, TValue>)currentNode;
        }

        public INode<TKey> ReadNode(int pageId)
        {
            var page = _pager.GetPage(pageId);
            var node = _serializer.Deserialize(page.Data);
            node.PageId = page.PageId;
            return node;
        }

        private void WriteNode(INode<TKey> node)
        {
            _serializer.Serialize(node, out var data);
            _pager.WritePage(node.PageId, data);
        }

        private void SetRootPageNumber(int pageId)
        {
            RootPageNumber = pageId;

            if (CatalogManager is CatalogManager manager && !string.IsNullOrWhiteSpace(TableName))
                manager.UpdateRootPageId(TableName, RootPageNumber);
        }
    }
}