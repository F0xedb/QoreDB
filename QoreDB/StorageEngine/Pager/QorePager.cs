namespace QoreDB.StorageEngine.Pager
{
    /// <summary>
    /// A concrete pager implementation that uses a physical file on disk for data storage.
    /// This pager is responsible for persisting data between application sessions.
    /// </summary>
    public class QorePager : BasePager
    {
        private readonly string _dbFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="QorePager"/> class.
        /// It opens or creates a database file at the specified path.
        /// </summary>
        /// <param name="dbFilePath">The path to the database file.</param>
        /// <param name="pageSize">The size of each page in bytes. Defaults to 4096.</param>
        /// <param name="maxPagesInCache">The maximum number of pages to hold in the in-memory cache. Defaults to 1024.</param>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist and cannot be created, or if permissions are denied.</exception>
        public QorePager(string dbFilePath, int pageSize = 4096, int maxPagesInCache = 1024)
            : base(new FileStream(dbFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite),
                  pageSize, maxPagesInCache)
        {
            if (!File.Exists(dbFilePath))
                throw new FileNotFoundException($"Can't open file {dbFilePath} either it doesn't exist or no permissions are granted");

            _dbFilePath = dbFilePath;
        }
    }
}
