using QoreDB.Catalog.Interfaces;
using QoreDB.Catalog.Models;
using QoreDB.StorageEngine;
using QoreDB.StorageEngine.Index;
using QoreDB.StorageEngine.Index.Interfaces;
using QoreDB.StorageEngine.Index.Serializer;
using QoreDB.StorageEngine.Index.Serializer.Implementations;
using QoreDB.StorageEngine.Pager.Interfaces;

namespace QoreDB.Catalog
{
    /// <summary>
    /// Manages the database catalog, which contains schema information
    /// </summary>
    /// <remarks>
    /// I've opted for a self referencing schema as database tables directly to allow extensibility
    /// </remarks>
    public class CatalogManager : ICatalogManager
    {
        // The B+ Trees now store the objects directly, serialization is handled by the NodeSerializer
        private readonly IBPlusTree<string, TableInfo> _tables;
        private readonly IBPlusTree<string, ColumnInfo> _columns;
        private readonly IPager _pager;

        public CatalogManager(IPager pager)
        {
            _pager = pager;

            var header = DatabaseHeader.Load(pager);

            var tableSerializer = new NodeSerializer<string, TableInfo>(new StringSerializer(), new TableInfoSerializer());
            var columnSerializer = new NodeSerializer<string, ColumnInfo>(new StringSerializer(), new ColumnInfoSerializer());

            _tables = new BackingStorageBPlusTree<string, TableInfo>(header.TablesRootPageId, Constants.DEFAULT_BTREE_PAGE_DEGREE, _pager, tableSerializer, this, nameof(_tables));
            _columns = new BackingStorageBPlusTree<string, ColumnInfo>(header.ColumnsRootPageId, Constants.DEFAULT_BTREE_PAGE_DEGREE, _pager, columnSerializer, this, nameof(_columns));
        }

        public void CreateTable(string tableName, IEnumerable<ColumnInfo> columns)
        {
            if (_tables.Search(tableName) is not null)
            {
                throw new Exception($"Table '{tableName}' already exists");
            }

            var newTableRootPage = _pager.AllocatePage();
            var columnList = columns.ToList();
            var tableInfo = new TableInfo(tableName, columnList, newTableRootPage);

            _tables.Insert(tableName, tableInfo);
            foreach (var column in columnList)
            {
                var columnKey = $"{tableName}_{column.ColumnName}";
                _columns.Insert(columnKey, column);
            }
        }

        public TableInfo? GetTable(string tableName)
            => _tables.Search(tableName);

        public void DropTable(string tableName, bool ifExists)
        {
            var table = GetTable(tableName);

            if (table == null && !ifExists)
                throw new Exception($"Table '{tableName}' not found");
            else if (table == null)
                return;

            _tables.Delete(tableName);
            foreach (var column in table.Columns)
            {
                var columnKey = $"{tableName}_{column.ColumnName}";
                _columns.Delete(columnKey);
            }
        }

        public void Insert<TKey>(string tableName, Dictionary<string, object> row)
            where TKey : IComparable<TKey>
        {
            var dataTree = GetTableTree<TKey>(tableName, out var tableInfo);

            var primaryKeyColumn = tableInfo.Columns[0];
            if (!row.TryGetValue(primaryKeyColumn.ColumnName, out var primaryKeyValue))
            {
                throw new ArgumentException($"Primary key '{primaryKeyColumn.ColumnName}' not found in row data");
            }

            var rowSerializer = new RowSerializer();
            var serializedRow = rowSerializer.Serialize(tableInfo, row);

            dataTree.Insert((TKey)primaryKeyValue, serializedRow);
        }

        public void UpdateRootPageId(string tableName, int RootPageId)
        {
            TableInfo table;

            // TODO: Allow updating the root page id of system tables
            if (tableName == nameof(_tables) || tableName == nameof(_columns))
                return;

            table = GetTable(tableName)
                ?? throw new ArgumentException($"Table {tableName} does not exit");

            var newTableInfo = new TableInfo(table.TableName, table.Columns, RootPageId);
            _tables.Insert(tableName, newTableInfo); // TODO: Don't insert but update in place instead
        }

        public IBPlusTree<TKey, byte[]> GetTableTree<TKey>(string tableName, out TableInfo tableInfo)
            where TKey : IComparable<TKey>
        {
            tableInfo = GetTable(tableName)
                ?? throw new Exception($"Table '{tableName}' not found");

            var keySerializer = GetSerializer<TKey>();
            var valueSerializer = new ByteArraySerializer();
            var nodeSerializer = new NodeSerializer<TKey, byte[]>(keySerializer, valueSerializer);
            return new BackingStorageBPlusTree<TKey, byte[]>(tableInfo.RootPage, Constants.DEFAULT_BTREE_PAGE_DEGREE, _pager, nodeSerializer, this, tableName);
        }

        private ISerializer<T> GetSerializer<T>() where T : IComparable<T>
        {
            if (typeof(T) == typeof(int))
            {
                return (ISerializer<T>)new IntSerializer();
            }
            if (typeof(T) == typeof(string))
            {
                return (ISerializer<T>)new StringSerializer();
            }
            throw new NotSupportedException($"No serializer found for key type {typeof(T).Name}");
        }

        public IEnumerable<TableInfo> GetAllTables()
            => _tables.GetAllValues();
    }
}