namespace QoreDB.StorageEngine.Pager
{
    /// <summary>
    /// A concrete pager implementation that uses an in-memory stream for data storage.
    /// This is useful for temporary databases or for unit testing the storage engine
    /// without hitting the file system.
    /// </summary>
    public class InMemoryQorePager : BasePager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryQorePager"/> class.
        /// </summary>
        /// <param name="stream">The memory stream to be used as the backing store.</param>
        /// <param name="pageSize">The size of each page in bytes. Defaults to 4096.</param>
        /// <param name="maxPagesInCache">The maximum number of pages to hold in the in-memory cache. Defaults to 1024.</param>
        public InMemoryQorePager(MemoryStream? stream = null, int pageSize = 4096, int maxPagesInCache = 1024)
            : base(stream ?? new MemoryStream(), pageSize, maxPagesInCache)
        {
        }
    }
}
