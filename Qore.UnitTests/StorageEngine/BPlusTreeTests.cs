using FluentAssertions;
using QoreDB.StorageEngine.Index;
using QoreDB.StorageEngine.Index.Nodes;

namespace Qore.UnitTests.StorageEngine
{
    [TestFixture]
    public class BPlusTreeTests
    {
        private BPlusTree<int, string> _tree;
        private const int TreeDegree = 3; // A small degree to force splits easily

        [SetUp]
        public void Setup()
        {
            _tree = new BPlusTree<int, string>(0, TreeDegree);
        }

        [Test]
        public void Constructor_WhenInitialized_ShouldHaveEmptyLeafNodeAsRoot()
        {
            // Assert
            _tree.Root.Should().NotBeNull();
            _tree.Root.IsLeaf.Should().BeTrue();
            _tree.Root.Keys.Should().BeEmpty();
            (_tree.Root as LeafNode<int, string>).Entries.Should().BeEmpty();
        }

        [Test]
        public void Search_InEmptyTree_ShouldReturnDefault()
        {
            // Act
            var result = _tree.Search(100);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void InsertAndSearch_SingleItem_ShouldBeFound()
        {
            // Arrange
            _tree.Insert(10, "Ten");

            // Act
            var foundValue = _tree.Search(10);
            var notFoundValue = _tree.Search(99);

            // Assert
            foundValue.Should().Be("Ten");
            notFoundValue.Should().BeNull();
        }

        [Test]
        public void Insert_WithinSingleLeafNode_ShouldStoreAllItems()
        {
            // Arrange (Degree is 3, so it can hold 2 keys)
            _tree.Insert(10, "Ten");
            _tree.Insert(20, "Twenty");

            // Act & Assert
            _tree.Root.IsLeaf.Should().BeTrue();
            _tree.Root.Keys.Count.Should().Be(2);
            _tree.Search(10).Should().Be("Ten");
            _tree.Search(20).Should().Be("Twenty");
        }

        [Test]
        public void Insert_CausesLeafSplit_ShouldCreateNewRootAndSplitKeys()
        {
            // Arrange (Degree is 3, so inserting 3rd key causes split)
            _tree.Insert(10, "Ten");
            _tree.Insert(30, "Thirty");
            _tree.Insert(20, "Twenty"); // Inserted out of order

            // Assert
            // 1. Root should now be an internal node
            _tree.Root.IsLeaf.Should().BeFalse();
            _tree.Root.Should().BeOfType<InternalNode<int>>();
            var root = (InternalNode<int>)_tree.Root;

            // 2. Root should have one key (the promoted key) and two children
            root.Keys.Count.Should().Be(1);
            root.Keys[0].Should().Be(20); // Middle key gets promoted
            root.Children.Count.Should().Be(2);

            // 3. Children should be leaf nodes with correct keys
            var leftChild = (LeafNode<int, string>)root.Children[0];
            var rightChild = (LeafNode<int, string>)root.Children[1];

            leftChild.Keys.Should().BeEquivalentTo([10]);
            leftChild.Entries.Select(e => e.Value).Should().BeEquivalentTo(["Ten"]);

            rightChild.Keys.Should().BeEquivalentTo([20, 30]);
            rightChild.Entries.Select(e => e.Value).Should().BeEquivalentTo(["Twenty", "Thirty"]);

            // 4. All values should still be searchable
            _tree.Search(10).Should().Be("Ten");
            _tree.Search(20).Should().Be("Twenty");
            _tree.Search(30).Should().Be("Thirty");
        }

        [Test]
        public void SiblingPointers_AfterLeafSplit_AreCorrectlyLinked()
        {
            // Arrange
            _tree.Insert(10, "Ten");
            _tree.Insert(20, "Twenty");
            _tree.Insert(30, "Thirty"); // Causes split

            // Act
            var root = (InternalNode<int>)_tree.Root;
            var leftLeaf = (LeafNode<int, string>)root.Children[0];
            var rightLeaf = (LeafNode<int, string>)root.Children[1];

            // Assert
            leftLeaf.NextSibling.Should().BeSameAs(rightLeaf);
            rightLeaf.PreviousSibling.Should().BeSameAs(leftLeaf);
            leftLeaf.PreviousSibling.Should().BeNull();
            rightLeaf.NextSibling.Should().BeNull();
        }

        [Test]
        public void Insert_CausesInternalNodeSplit_ShouldIncreaseTreeHeight()
        {
            // Arrange: Insert enough items to cause multiple splits, leading to an internal node split.
            // With degree 3, this happens on the 7th item.
            // 10, 20, 30 -> split leaf, root has [2]
            // 40, 50 -> right leaf splits, root has [2, 3]
            // 60, 70 -> right-right leaf splits, root's right child splits, root becomes full 
            // Now, inserting 5 will cause the leftmost leaf to split, promoting 10.
            // This forces the root  to split.
            _tree.Insert(10, "10");
            _tree.Insert(20, "20");
            _tree.Insert(30, "30");
            _tree.Insert(40, "40");
            _tree.Insert(50, "50");
            _tree.Insert(60, "60");
            _tree.Insert(5, "5"); // This will cause the final split of the root

            // Assert
            // 1. New root should be an internal node with one key
            _tree.Root.IsLeaf.Should().BeFalse();
            _tree.Root.Keys.Count.Should().Be(1);
            _tree.Root.Keys[0].Should().Be(30); // The middle key of the old root is promoted

            // 2. The new root's children should be internal nodes
            var root = (InternalNode<int>)_tree.Root;
            root.Children[0].Should().BeOfType<InternalNode<int>>();
            root.Children[1].Should().BeOfType<InternalNode<int>>();

            // 3. Verify all items are still searchable
            _tree.Search(5).Should().Be("5");
            _tree.Search(10).Should().Be("10");
            _tree.Search(20).Should().Be("20");
            _tree.Search(30).Should().Be("30");
            _tree.Search(40).Should().Be("40");
            _tree.Search(50).Should().Be("50");
            _tree.Search(60).Should().Be("60");
        }

        [Test]
        public void Insert_ManySequentialItems_ShouldRemainBalancedAndSearchable()
        {
            // Arrange
            int itemCount = 100;
            for (int i = 1; i <= itemCount; i++)
            {
                _tree.Insert(i, i.ToString());
            }

            // Act & Assert
            for (int i = 1; i <= itemCount; i++)
            {
                _tree.Search(i).Should().Be(i.ToString(), $"because item {i} should be found");
            }
            _tree.Search(itemCount + 1).Should().BeNull();
            _tree.Search(0).Should().BeNull();
        }

        [Test]
        public void Delete_ExistingKey_ShouldRemoveEntry()
        {
            // Arrange
            _tree.Insert(10, "Ten");
            _tree.Insert(20, "Twenty");
            _tree.Search(10).Should().NotBeNull();

            // Act
            _tree.Delete(10);

            // Assert
            _tree.Search(10).Should().BeNull();
            _tree.Search(20).Should().Be("Twenty"); // Other item should remain
        }

        [Test]
        public void Delete_NonExistentKey_ShouldNotChangeTree()
        {
            // Arrange
            _tree.Insert(10, "Ten");
            _tree.Insert(20, "Twenty");

            // Act
            Action deleteAction = () => _tree.Delete(99);

            // Assert
            deleteAction.Should().NotThrow();
            _tree.Search(10).Should().Be("Ten");
            _tree.Search(20).Should().Be("Twenty");
        }

        [Test]
        public void Insert_WhenNodeSplits_RootBecomesInternalNode()
        {
            // Arrange
            var tree = new BPlusTree<int, string>(1, 3);

            // Act
            tree.Insert(10, "ten");
            tree.Insert(20, "twenty");
            tree.Insert(30, "thirty"); // This should cause a split

            // Assert
            tree.Root.Should().BeOfType<InternalNode<int>>();
            tree.Root.IsLeaf.Should().BeFalse();
            tree.Root.Keys.Count.Should().Be(1);
            tree.Root.Keys[0].Should().Be(20);

            // Verify data is still searchable
            tree.Search(10).Should().Be("ten");
            tree.Search(20).Should().Be("twenty");
            tree.Search(30).Should().Be("thirty");
        }

        [Test]
        public void GetAllValues_WhenTreeHasDataAcrossMultipleNodes_ReturnsAllValuesInOrder()
        {
            // Arrange
            var tree = new BPlusTree<int, string>(1, 3);
            tree.Insert(10, "ten");
            tree.Insert(20, "twenty");
            tree.Insert(5, "five");
            tree.Insert(30, "thirty");
            tree.Insert(15, "fifteen");

            // Act
            var values = tree.GetAllValues().ToList();

            // Assert
            values.Should().HaveCount(5);
            values.Should().ContainInOrder("five", "ten", "fifteen", "twenty", "thirty");
        }
    }
}
