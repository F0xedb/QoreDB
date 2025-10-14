using FluentAssertions;
using QoreDB.StorageEngine.Pager;
using System.Text;

namespace Qore.UnitTests.StorageEngine
{
    [TestFixture]
    public class PagerTests
    {
        private const int PageSize = 128; // Use a small page size for easier testing
        private const int CacheSize = 3;   // Use a small cache size to test eviction
        private MemoryStream _memoryStream;
        private InMemoryQorePager _pager;

        [SetUp]

        public void Setup()
        {
            _memoryStream = new MemoryStream();
            _pager = new InMemoryQorePager(_memoryStream, PageSize, CacheSize);
        }

        [TearDown]

        public void Teardown()
        {
            _pager?.Dispose();

            if (_memoryStream.CanRead)
                _memoryStream?.Dispose();
        }

        private byte[] CreateSamplePageData(int pageId)
        {
            var data = new byte[_pager.PageSize];
            var content = $"This is page {pageId}";
            Encoding.UTF8.GetBytes(content).CopyTo(data, 0);
            return data;
        }

        // =================================================================
        // Basic I/O and Allocation Tests
        // =================================================================

        [Test]
        public void AllocatePage_OnNewPager_ReturnsSequentialIds()
        {
            // Act & Assert
            _pager.AllocatePage().Should().Be(0);
            _pager.AllocatePage().Should().Be(1);
            _pager.AllocatePage().Should().Be(2);
        }

        [Test]
        public void WritePageAndReadPage_WhenBypassingCache_ShouldReturnCorrectData()
        {
            // Arrange
            var pageId = 0;
            var dataToWrite = CreateSamplePageData(pageId);

            // Act
            _pager.WritePage(pageId, dataToWrite);
            var dataRead = _pager.ReadPage(pageId);

            // Assert
            dataRead.Should().BeEquivalentTo(dataToWrite);
        }

        [Test]
        public void ReadPage_WhenPageDoesNotExist_ShouldReturnBlankPage()
        {
            // Arrange
            var nonExistentPageId = 99;
            var expectedBlankPage = new byte[_pager.PageSize];

            // Act
            var dataRead = _pager.ReadPage(nonExistentPageId);

            // Assert
            dataRead.Should().BeEquivalentTo(expectedBlankPage);
        }

        // =================================================================
        // Caching Logic Tests (GetPage)
        // =================================================================
        [Test]
        public void GetPage_WhenPageIsNotInCache_ShouldLoadFromStreamAndAddToCache()
        {
            // Arrange
            var pageId = 0;
            var pageData = CreateSamplePageData(pageId);
            _pager.WritePage(pageId, pageData); // Pre-populate the stream

            // Act
            var page = _pager.GetPage(pageId);

            // Assert
            page.Should().NotBeNull();
            page.PageId.Should().Be(pageId);
            page.Data.Should().BeEquivalentTo(pageData);
        }

        [Test]
        public void GetPage_WhenPageIsInCache_ShouldReturnCachedPageWithoutReadingStream()
        {
            // Arrange
            var pageId = 0;
            var originalData = CreateSamplePageData(pageId);
            _pager.WritePage(pageId, originalData);

            // Act
            var firstGet = _pager.GetPage(pageId); // First call, loads into cache

            // Modify the underlying stream directly to simulate an out-of-band change
            var modifiedData = CreateSamplePageData(999);
            _memoryStream.Write(modifiedData);

            var secondGet = _pager.GetPage(pageId); // Second call, should be a cache hit

            // Mark the cache as invalid by writing a page
            _pager.WritePage(pageId, modifiedData);

            var thirdGet = _pager.GetPage(pageId);

            // Assert
            firstGet.Data.Should().BeEquivalentTo(originalData);
            secondGet.Data.Should().BeEquivalentTo(originalData, "because it should be a cache hit");
            thirdGet.Data.Should().NotBeEquivalentTo(originalData);
        }

        // =================================================================
        // Cache Eviction Tests
        // =================================================================
        [Test]
        public void GetPage_WhenCacheIsFull_ShouldEvictLeastRecentlyUsedPage()
        {
            // Arrange: Fill the cache (CacheSize = 3)
            _pager.GetPage(0); // LRU
            _pager.GetPage(1);
            _pager.GetPage(2); // MRU

            // Act: Access page 0 to make it the MRU, making page 1 the new LRU
            _pager.GetPage(0);

            // Request a new page to force eviction of the LRU page (page 1)
            _pager.GetPage(3);

            // To verify eviction, we check if getting page 1 now results in a read
            // from a modified stream.
            var modifiedPage1Data = CreateSamplePageData(111);
            _pager.WritePage(1, modifiedPage1Data); // Modify page 1 in the stream

            var page1AfterEviction = _pager.GetPage(1);

            // Assert
            page1AfterEviction.Data.Should().BeEquivalentTo(modifiedPage1Data,
                "because page 1 should have been evicted and re-read from the stream");
        }

        [Test]
        public void GetPage_WhenEvictingDirtyPage_ShouldWriteChangesToStream()
        {
            // Arrange: Fill the cache
            var page0 = _pager.GetPage(0); // This will become the LRU and we will make it dirty
            _pager.GetPage(1);
            _pager.GetPage(2);

            // Modify page 0 and mark it as dirty
            var modifiedData = CreateSamplePageData(100);
            Array.Copy(modifiedData, page0.Data, PageSize);
            page0.IsDirty = true;

            // Act: Request a new page to force eviction of dirty page 0
            _pager.GetPage(3);

            // Assert: Read the page directly from the stream to verify it was written
            var dataFromStream = _pager.ReadPage(0);
            dataFromStream.Should().BeEquivalentTo(modifiedData,
                "because the dirty page should have been flushed to the stream on eviction");
        }

        // =================================================================
        // Flush and Dispose Tests
        // =================================================================
        [Test]
        public void FlushPage_WhenPageIsDirty_ShouldWriteToStreamAndMarkAsClean()
        {
            // Arrange
            var page = _pager.GetPage(0);
            var modifiedData = CreateSamplePageData(100);
            Array.Copy(modifiedData, page.Data, PageSize);
            page.IsDirty = true;

            // Act
            _pager.FlushPage(0);

            // Assert
            var dataFromStream = _pager.ReadPage(0);
            dataFromStream.Should().BeEquivalentTo(modifiedData);
            page.IsDirty.Should().BeFalse("because flushing should mark the page as clean");
        }

        [Test]
        public void FlushPage_WhenPageIsNotDirty_ShouldDoNothing()
        {
            // Arrange
            var originalData = CreateSamplePageData(0);
            _pager.WritePage(0, originalData);
            var page = _pager.GetPage(0); // Page is now in cache, IsDirty = false

            // Modify the stream data behind the cache's back
            var modifiedStreamData = CreateSamplePageData(999);
            _pager.WritePage(0, modifiedStreamData);

            // Act
            _pager.FlushPage(0);

            // Assert
            var dataFromStream = _pager.ReadPage(0);
            dataFromStream.Should().BeEquivalentTo(modifiedStreamData,
                "because flushing a clean page should not write to the stream");
        }

        [Test]
        public void Dispose_WhenDirtyPagesExist_ShouldFlushAllDirtyPages()
        {
            // Arrange
            var page0 = _pager.GetPage(0);
            var page1 = _pager.GetPage(1);
            var page2 = _pager.GetPage(2);

            // Make pages 0 and 2 dirty, leave page 1 clean
            var modifiedData0 = CreateSamplePageData(100);
            Array.Copy(modifiedData0, page0.Data, PageSize);
            page0.IsDirty = true;

            var modifiedData2 = CreateSamplePageData(200);
            Array.Copy(modifiedData2, page2.Data, PageSize);
            page2.IsDirty = true;

            // Act
            _pager.Dispose();

            // Assert
            // We must re-open the stream to check its contents as Dispose closes it.
            using var newStream = new MemoryStream(_memoryStream.ToArray());
            using var newPager = new InMemoryQorePager(newStream, PageSize, CacheSize);

            newPager.ReadPage(0).Should().BeEquivalentTo(modifiedData0, "because dirty page 0 should have been flushed");
            newPager.ReadPage(1).Should().BeEquivalentTo(new byte[_pager.PageSize], "because clean page 1 should not have been flushed");
            newPager.ReadPage(2).Should().BeEquivalentTo(modifiedData2, "because dirty page 2 should have been flushed");
        }

        // =================================================================
        // Page Existence Tests (PageExists)
        // =================================================================

        [Test]
        public void PageExists_WhenPageHasBeenAllocated_ShouldReturnTrue()
        {
            // Arrange
            var pageId = _pager.AllocatePage(); // pageId is 0

            // Act
            var exists = _pager.PageExists(pageId);

            // Assert
            exists.Should().BeTrue();
        }

        [Test]
        public void PageExists_WhenPageHasNotBeenAllocated_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentPageId = 42;

            // Act
            var exists = _pager.PageExists(nonExistentPageId);

            // Assert
            exists.Should().BeFalse();
        }

        [Test]
        public void PageExists_WithNegativeId_ShouldReturnFalse()
        {
            // Act
            var exists = _pager.PageExists(-1);

            // Assert
            exists.Should().BeFalse();
        }
    }
}
