using QoreDB.Catalog.Models;
using QoreDB.StorageEngine.Index.Interfaces;

namespace QoreDB.Catalog.Interfaces
{
    /// <summary>
    /// Manages the database catalog, which contains schema information
    /// </summary>
    public interface ICatalogManager
    {
        /// <summary>
        /// Creates a new table in the database
        /// </summary>
        /// <param name="tableName">The name of the table to create</param>
        /// <param name="columns">A list of columns in the table</param>
        void CreateTable(string tableName, IEnumerable<ColumnInfo> columns);

        /// <summary>
        /// Gets information about a table
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <returns>A TableInfo object, or null if the table does not exist</returns>
        TableInfo? GetTable(string tableName);

        /// <summary>
        /// Get the backing <see cref="IBPlusTree{TKey, TValue}"/> of the table <paramref name="tableName"/>
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <param name="tableInfo">The table info stored in the system table</param>
        /// <returns>The B+ tree of the table</returns>
        public IBPlusTree<TKey, byte[]> GetTableTree<TKey>(string tableName, out TableInfo tableInfo)
            where TKey : IComparable<TKey>;

        /// <summary>
        /// Deletes a table from the database
        /// </summary>
        /// <param name="tableName">The name of the table to delete</param>
        /// <param name="ifExists">If we should drop the table if it already exists</param>
        void DropTable(string tableName, bool ifExists);

        /// <summary>
        /// Inserts a row of data into the specified table
        /// </summary>
        /// <typeparam name="TKey">The data type of the table's primary key</typeparam>
        /// <param name="tableName">The name of the table to insert into</param>
        /// <param name="row">A dictionary representing the row, with column names as keys</param>
        public void Insert<TKey>(string tableName, Dictionary<string, object> row)
            where TKey : IComparable<TKey>;

        /// <summary>
        /// Retrieves a list of all tables in the database
        /// </summary>
        /// <returns>An enumerable collection of TableInfo objects</returns>
        IEnumerable<TableInfo> GetAllTables();
    }
}
