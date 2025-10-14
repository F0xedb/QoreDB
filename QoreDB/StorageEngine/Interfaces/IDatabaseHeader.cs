namespace QoreDB.StorageEngine.Interfaces
{
    /// <summary>
    /// Defines the contract for the database header
    /// </summary>
    internal interface IDatabaseHeader
    {
        /// <summary>
        /// The page number of the root of the tables B+ Tree
        /// </summary>
        int TablesRootPageId { get; }
        
        /// <summary>
        /// The page number of the root of the columns B+ Tree
        /// </summary>
        int ColumnsRootPageId { get; }
    }
}