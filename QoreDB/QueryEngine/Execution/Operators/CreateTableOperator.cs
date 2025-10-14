using QoreDB.Catalog.Models;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;
using System.Collections.Generic;

namespace QoreDB.QueryEngine.Execution.Operators
{
    /// <summary>
    /// An operator that creates a new table in the catalog
    /// </summary>
    public class CreateTableOperator : BaseExecutionOperator
    {
        public override string Name => $"CreateTable({_tableName})";

        public override IExecutionOperator Source => null; // This is a root operator
        private readonly string _tableName;
        private readonly IEnumerable<ColumnInfo> _columns;

        public CreateTableOperator(string tableName, IEnumerable<ColumnInfo> columns)
        {
            _tableName = tableName;
            _columns = columns;
        }

        protected override IQueryResult ExecuteInternal(IExecutionContext context)
        {
            context.Catalog.CreateTable(_tableName, _columns);
            return new MessageQueryResult($"Table '{_tableName}' created successfully");
        }

        public override IExecutionOperator CopyWithNewSource(IExecutionOperator newSource)
            => new CreateTableOperator(_tableName, _columns);
    }
}