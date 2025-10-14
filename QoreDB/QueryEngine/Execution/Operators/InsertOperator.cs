using QoreDB.Catalog.Interfaces;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;
using System;
using System.Collections.Generic;

namespace QoreDB.QueryEngine.Execution.Operators
{
    /// <summary>
    /// An operator that inserts a single row into a table
    /// </summary>
    public class InsertOperator : BaseExecutionOperator
    {
        public override string Name => $"Insert({_tableName})";

        public override IExecutionOperator Source => null; // This is a root operator
        private readonly string _tableName;
        private readonly Dictionary<string, object> _row;

        public InsertOperator(string tableName, Dictionary<string, object> row)
        {
            _tableName = tableName;
            _row = row;
        }

        protected override IQueryResult ExecuteInternal(IExecutionContext context)
        {
            var tableInfo = context.Catalog.GetTable(_tableName)
                ?? throw new Exception($"Table '{_tableName}' not found");

            var primaryKeyType = tableInfo.Columns[0].DataType;

            var method = context.Catalog.GetType().GetMethod(nameof(ICatalogManager.Insert));
            var genericMethod = method.MakeGenericMethod(primaryKeyType);
            genericMethod.Invoke(context.Catalog, new object[] { _tableName, _row });

            return new MessageQueryResult("1 row inserted");
        }
        
        public override IExecutionOperator CopyWithNewSource(IExecutionOperator newSource)
            => new InsertOperator(_tableName, _row);
    }
}