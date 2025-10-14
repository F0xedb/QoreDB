using QoreDB.StorageEngine.Pager.Models;

namespace QoreDB.StorageEngine.Pager.Interfaces
{
    /// <summary>
    /// Defines the core contract for a pager component, which is responsible for
    /// reading and writing fixed-size pages to and from an underlying data stream.
    /// This interface abstracts the direct interaction with the data source.
    /// </summary>
    public interface IPager : IDisposable
    {
        /// <summary>
        /// The total size of the database file in bytes
        /// </summary>
        long FileSize { get; }

        /// <summary>
        /// The total amount of pages allocated
        /// </summary>
        long AllocatedPages { get; }

        /// <summary>
        /// Reads the raw byte data of a specific page directly from the underlying data stream.
        /// This method bypasses any caching layers.
        /// </summary>
        /// <param name="pageId">The unique identifier of the page to read.</param>
        /// <returns>A byte array containing the data of the page. If the page does not exist,
        /// an empty (all-zero) byte array is returned.</returns>
        byte[] ReadPage(int pageId);

        /// <summary>
        /// Writes raw byte data for a specific page directly to the underlying data stream.
        /// This method bypasses any caching layers.
        /// </summary>
        /// <param name="pageId">The unique identifier of the page to write.</param>
        /// <param name="data">The byte array of data to be written. The array size must match the pager's page size.</param>
        void WritePage(int pageId, byte[] data);

        /// <summary>
        /// Allocates space for a new page in the data stream and returns its new page ID.
        /// This typically involves extending the size of the underlying stream.
        /// </summary>
        /// <returns>The unique identifier for the newly allocated page.</returns>
        int AllocatePage();

        /// <summary>
        /// Retrieves a page by its ID, utilizing an in-memory cache for performance.
        /// If the page is present in the cache (a cache hit), it is returned directly.
        /// If not (a cache miss), it is read from the underlying stream, loaded into the cache, and then returned.
        /// </summary>
        /// <param name="pageId">The unique identifier of the page to retrieve.</param>
        /// <returns>A <see cref="QorePage"/> object representing the requested page.</returns>
        QorePage GetPage(int pageId);

        /// <summary>
        /// Checks if a page with the specified <paramref name="pageId"/> has been allocated in the backing store
        /// </summary>
        /// <param name="pageId">The zero-based ID of the page to check</param>
        /// <returns><see langword="true"/> if the page exists; otherwise, <see langword="false"/></returns>
        bool PageExists(int pageId);
    }
}
