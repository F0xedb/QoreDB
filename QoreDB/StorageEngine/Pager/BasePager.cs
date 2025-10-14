using QoreDB.StorageEngine.Pager.Interfaces;
using QoreDB.StorageEngine.Pager.Models;

namespace QoreDB.StorageEngine.Pager
{
    /// <summary>
    /// Provides a base implementation for a pager that manages reading and writing
    /// pages to an underlying stream, with an integrated LRU (Least Recently Used) caching mechanism.
    /// This class is abstract and must be inherited by a concrete pager implementation
    /// that provides the specific stream (e.g., FileStream or MemoryStream).
    /// </summary>
    public abstract class BasePager : ICachedPager
    {
        /// <summary>
        /// The fixed size of each page in bytes.
        /// </summary>
        public readonly int PageSize;

        private Stream _dataStream;
        private long _dataStreamSize;

        public long FileSize
            => _dataStreamSize;

        public long AllocatedPages
            => FileSize / PageSize;

        public long CachedPages
            => _pageCache.Count;

        private readonly int _maxPagesInCache;
        private readonly Dictionary<int, QorePage> _pageCache;
        private readonly LinkedList<int> _lruList;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePager"/> class.
        /// </summary>
        /// <param name="stream">The underlying data stream to read from and write to.</param>
        /// <param name="pageSize">The size of each page in bytes. Defaults to 4096.</param>
        /// <param name="maxPagesInCache">The maximum number of pages to hold in the in-memory cache. Defaults to 1024.</param>
        public BasePager(Stream emptyStream, int pageSize = Constants.DEFAULT_PAGE_SIZE, int maxPagesInCache = Constants.DEFAULT_PAGE_CACHE_SIZE)
        {
            PageSize = pageSize;
            _maxPagesInCache = maxPagesInCache;

            _pageCache = new Dictionary<int, QorePage>();
            _lruList = new LinkedList<int>();

            _dataStream = emptyStream;
            _dataStreamSize = _dataStream.Length;

            _disposed = false;
        }

        public byte[] ReadPage(int pageId)
        {
            byte[] pageData = new byte[PageSize];
            var offset = (long)pageId * PageSize;

            if (offset >= _dataStreamSize)
            {
                // This page doesn't exist yet, return a blank page.
                return pageData;
            }

            _dataStream.Seek(offset, SeekOrigin.Begin);
            _dataStream.Read(pageData, 0, PageSize);
            return pageData;
        }

        public void WritePage(int pageId, byte[] data)
        {
            var offset = (long)pageId * PageSize;
            _dataStream.Seek(offset, SeekOrigin.Begin);
            _dataStream.Write(data, 0, PageSize);
            _dataStreamSize = _dataStream.Length;

            // Cache hit: let's remove the page and mark the in-memory references as dirty
            if (_pageCache.TryGetValue(pageId, out QorePage page))
            {
                page.IsDirty = true;
                _lruList.Remove(pageId);
                _pageCache.Remove(pageId);
            }
        }

        public QorePage GetPage(int pageId)
        {
            if (_pageCache.TryGetValue(pageId, out QorePage page))
            {
                // Cache hit: move page to front of LRU list
                _lruList.Remove(pageId);
                _lruList.AddFirst(pageId);
                return page;
            }

            // Cache miss: load from disk
            if (_pageCache.Count >= _maxPagesInCache)
            {
                EvictPage();
            }

            var pageData = ReadPage(pageId);
            page = new QorePage(pageId, PageSize);
            Array.Copy(pageData, page.Data, PageSize);

            _pageCache.Add(pageId, page);
            _lruList.AddFirst(pageId);

            return page;
        }

        public int AllocatePage()
        {
            int newPageId = (int)(_dataStreamSize / PageSize);
            _dataStreamSize += PageSize;
            return newPageId;
        }

        public void EvictPage()
        {
            if (_lruList.Last == null)
                return;

            int pageIdToEvict = _lruList.Last.Value;
            _lruList.RemoveLast();

            var pageToEvict = _pageCache[pageIdToEvict];
            _pageCache.Remove(pageIdToEvict);

            if (pageToEvict.IsDirty)
            {
                WritePage(pageToEvict.PageId, pageToEvict.Data);
            }
        }

        public void FlushPage(int pageId)
        {
            if (_pageCache.TryGetValue(pageId, out QorePage page) && page.IsDirty)
            {
                WritePage(page.PageId, page.Data);
                page.IsDirty = false;
            }
        }

        public bool PageExists(int pageId)
        {
            if (pageId < 0) return false;

            if (_pageCache.ContainsKey(pageId)) return true;

            // Check if the end of the requested page is within the stream's length
            return _dataStreamSize >= (long)(pageId + 1) * PageSize;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            foreach (var page in _pageCache.Values)
            {
                if (page.IsDirty)
                {
                    WritePage(page.PageId, page.Data);
                }
            }

            _dataStream?.Flush();
            _dataStream?.Close();
            _dataStream?.Dispose();

            _disposed = true;
        }
    }
}
