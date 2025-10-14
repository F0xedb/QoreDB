namespace QoreDB.StorageEngine.Pager.Models
{
    /// <summary>
    /// Represents a single, fixed-size page of data held in memory.
    /// This class acts as the unit of transfer between the storage engine and the in-memory cache.
    /// </summary>
    public class QorePage
    {
        /// <summary>
        /// Gets the raw byte array containing the page's data.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the unique, zero-based identifier for this page.
        /// </summary>
        public int PageId { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the page's data has been modified in memory
        /// since it was loaded from the underlying stream. A dirty page needs to be written
        /// back to the stream to persist its changes.
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QorePage"/> class.
        /// </summary>
        /// <param name="pageId">The unique identifier for the page.</param>
        /// <param name="pageSize">The size of the page's data buffer in bytes.</param>
        public QorePage(int pageId, int pageSize)
        {
            PageId = pageId;
            Data = new byte[pageSize];
            IsDirty = false;
        }
    }
}
