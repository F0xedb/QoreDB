namespace QoreDB.Catalog.Models
{
    /// <summary>
    /// Contains information about a column in a table
    /// </summary>
    public class ColumnInfo
    {
        /// <summary>
        /// The name of the column
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// The data type of the column
        /// </summary>
        public Type DataType { get; }

        public ColumnInfo(string columnName, Type dataType)
        {
            ColumnName = columnName;
            DataType = dataType;
        }
    }
}
