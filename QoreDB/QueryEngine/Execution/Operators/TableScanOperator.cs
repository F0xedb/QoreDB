using QoreDB.Catalog.Interfaces;
using QoreDB.Catalog.Models;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;
using QoreDB.StorageEngine.Index.Interfaces;
using QoreDB.StorageEngine.Index.Serializer.Implementations;

namespace QoreDB.QueryEngine.Execution.Operators
{
    /// <summary>
    /// An operator that performs a full scan of a table
    /// </summary>
    public class TableScanOperator : BaseExecutionOperator
    {
        public override string Name => $"TableScan({_tableName})";

        public override IExecutionOperator Source => null; // This is a source operator
        private readonly string _tableName;

        public TableScanOperator(string tableName)
        {
            _tableName = tableName;
        }

        protected override IQueryResult ExecuteInternal(IExecutionContext context)
        {
            return new RowsQueryResult(ScanTable(context));
        }

        private IEnumerable<Dictionary<string, object>> ScanTable(IExecutionContext context)
        {
            // Get the table info first
            var initialTableInfo = context.Catalog.GetTable(_tableName)
                ?? throw new Exception($"Table '{_tableName}' not found");

            if (!initialTableInfo.Columns.Any())
                yield break;

            var primaryKeyType = initialTableInfo.Columns[0].DataType;

            var getTableTreeMethod = context.Catalog.GetType().GetMethod(nameof(ICatalogManager.GetTableTree));
            var genericMethod = getTableTreeMethod.MakeGenericMethod(primaryKeyType);

            var methodParams = new object[] { _tableName, null };
            var dataTree = genericMethod.Invoke(context.Catalog, methodParams);
            var tableInfo = (TableInfo)methodParams[1]; // Retrieve the 'out' parameter value

            var getAllMethod = dataTree.GetType().GetMethod(nameof(IBPlusTree<int, byte[]>.GetAllValues));
            var allValues = (IEnumerable<byte[]>)getAllMethod.Invoke(dataTree, null);

            var rowSerializer = new RowSerializer();

            foreach (var serializedRow in allValues)
            {
                yield return rowSerializer.Deserialize(tableInfo, serializedRow);
            }
        }
        
        public override IExecutionOperator CopyWithNewSource(IExecutionOperator newSource)
            => new TableScanOperator(_tableName);
    }
}