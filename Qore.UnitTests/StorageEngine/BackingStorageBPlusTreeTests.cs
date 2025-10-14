using FluentAssertions;
using QoreDB.Catalog;
using QoreDB.StorageEngine.Index;
using QoreDB.StorageEngine.Index.Nodes;
using QoreDB.StorageEngine.Index.Serializer;
using QoreDB.StorageEngine.Index.Serializer.Implementations;
using QoreDB.StorageEngine.Pager;

namespace Qore.UnitTests.StorageEngine
{
    [TestFixture]
    public class BackingStorageBPlusTreeTests
    {
        private InMemoryQorePager _pager;
        private NodeSerializer<int, string> _nodeSerializer;

        [SetUp]
        public void SetUp()
        {
            _pager = new InMemoryQorePager();
            var keySerializer = new IntSerializer();
            var valueSerializer = new StringSerializer();
            _nodeSerializer = new NodeSerializer<int, string>(keySerializer, valueSerializer);
        }

        [TearDown]
        public void TearDown()
        {
            _pager.Dispose();
        }

        [Test]
        public void Constructor_WhenCreatingNewTree_InitializesCorrectly()
        {
            // Arrange & Act
            _pager.PageExists(0).Should().BeFalse();
            var tree = new BackingStorageBPlusTree<int, string>(1, 3, _pager, _nodeSerializer, null, null);

            // Assert
            tree.RootPageNumber.Should().Be(0, "because the first page should be allocated for the root in the pager");
            tree.Root.Should().BeOfType<LeafNode<int, string>>();
            tree.Root.IsLeaf.Should().BeTrue();
            tree.Root.Keys.Should().BeEmpty();
            _pager.PageExists(0).Should().BeTrue();
        }

        [Test]
        public void InsertAndSearch_SingleItem_ReturnsCorrectValue()
        {
            // Arrange
            var tree = new BackingStorageBPlusTree<int, string>(0, 3, _pager, _nodeSerializer, new CatalogManager(_pager), null);

            // Act
            tree.Insert(10, "ten");
            var result = tree.Search(10);

            // Assert
            result.Should().Be("ten");
        }

        [Test]
        public void Insert_WhenNodeSplits_RootBecomesInternalNode()
        {
            // Arrange
            // A degree of 3 means a node splits when it has 3 keys
            var tree = new BackingStorageBPlusTree<int, string>(1, 3, _pager, _nodeSerializer, new CatalogManager(_pager), null);

            // Act
            tree.Insert(10, "ten");
            tree.Insert(20, "twenty");
            tree.Insert(30, "thirty"); // This should cause a split

            // Assert
            tree.Root.Should().BeOfType<InternalNode<int>>("because the root should have split");
            tree.Root.IsLeaf.Should().BeFalse();
            tree.Root.Keys.Count.Should().Be(1);

            // Verify all data is still searchable
            tree.Search(10).Should().Be("ten");
            tree.Search(20).Should().Be("twenty");
            tree.Search(30).Should().Be("thirty");
        }

        [Test]
        public void Persistence_WhenTreeIsReloaded_DataIsPreserved()
        {
            // Arrange
            var initialTree = new BackingStorageBPlusTree<int, string>(0, 3, _pager, _nodeSerializer, new CatalogManager(_pager), null);
            initialTree.Insert(10, "ten");
            initialTree.Insert(20, "twenty");
            initialTree.Insert(5, "five");
            initialTree.Insert(15, "fifteen");
            initialTree.Insert(25, "twenty-five");

            var finalRootPage = initialTree.RootPageNumber;

            // Act: Create a new tree instance using the same pager and root page
            var reloadedTree = new BackingStorageBPlusTree<int, string>(finalRootPage, 3, _pager, _nodeSerializer, new CatalogManager(_pager), null);

            // Assert: All data should be present in the reloaded tree
            reloadedTree.Search(5).Should().Be("five");
            reloadedTree.Search(10).Should().Be("ten");
            reloadedTree.Search(15).Should().Be("fifteen");
            reloadedTree.Search(20).Should().Be("twenty");
            reloadedTree.Search(25).Should().Be("twenty-five");
            reloadedTree.RootPageNumber.Should().Be(finalRootPage);
        }

        [Test]
        public void GetAllValues_WhenTreeIsPersisted_ReturnsAllValuesInOrder()
        {
            // Arrange
            var tree = new BackingStorageBPlusTree<int, string>(0, 3, _pager, _nodeSerializer, new CatalogManager(_pager), null);
            // Insert enough items to cause at least one leaf node split
            tree.Insert(10, "ten");
            tree.Insert(20, "twenty");
            tree.Insert(5, "five");
            tree.Insert(30, "thirty");
            tree.Insert(15, "fifteen");
            tree.Insert(25, "twenty-five");

            // Act
            var values = tree.GetAllValues().ToList();

            // Assert
            values.Should().HaveCount(6);
            values.Should().ContainInOrder("five", "ten", "fifteen", "twenty", "twenty-five", "thirty");
        }
    }
}
