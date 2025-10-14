namespace QoreDB.StorageEngine.Pager.Interfaces
{
    /// <summary>
    /// Extends the <see cref="IPager"/> interface with methods for explicit cache management.
    /// Implementations of this interface provide control over the caching behavior,
    /// such as forcing page eviction or flushing changes to the underlying stream.
    /// </summary>
    internal interface ICachedPager : IPager
    {
        /// <summary>
        /// Manually evicts a page from the cache based on the cache's eviction policy (e.g., LRU).
        /// If the evicted page is dirty, its contents are written to the underlying stream before removal.
        /// </summary>
        void EvictPage();

        /// <summary>
        /// Writes the contents of a specific page from the cache to the underlying data stream if it is marked as dirty.
        /// After a successful flush, the page is marked as clean (IsDirty = false).
        /// </summary>
        /// <param name="pageId">The unique identifier of the page to flush.</param>
        void FlushPage(int pageId);

        /// <summary>
        /// Pages present in the cache
        /// </summary>
        long CachedPages { get; }
    }
}
