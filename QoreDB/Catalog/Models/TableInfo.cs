namespace QoreDB.Catalog.Models
{
    /// <summary>
    /// Contains information about a table
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// The name of the table
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// The columns in the table
        /// </summary>
        public IReadOnlyList<ColumnInfo> Columns { get; }

        /// <summary>
        /// The root page of the table's data in the storage engine
        /// </summary>-
        public int RootPage { get; }

        public TableInfo(string tableName, IReadOnlyList<ColumnInfo> columns, int rootPage)
        {
            TableName = tableName;
            Columns = columns;
            RootPage = rootPage;
        }
    }
}
