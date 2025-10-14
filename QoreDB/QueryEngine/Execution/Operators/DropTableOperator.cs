using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;

namespace QoreDB.QueryEngine.Execution.Operators
{
    /// <summary>
    /// An operator that drops a table from the catalog
    /// </summary>
    public class DropTableOperator : BaseExecutionOperator
    {

        public override string Name => $"DropTable({_tableName}, IfExists={_ifExists})";

        public override IExecutionOperator Source => null; // This is a root operator

        private readonly string _tableName;

        /// <summary>
        /// If true, the operator will not throw an error if the table does not exist
        /// </summary>
        private readonly bool _ifExists;

        public DropTableOperator(string tableName, bool ifExists)
        {
            _tableName = tableName;
            _ifExists = ifExists;
        }

        protected override IQueryResult ExecuteInternal(IExecutionContext context)
        {
            context.Catalog.DropTable(_tableName, _ifExists);
            return new MessageQueryResult($"Table '{_tableName}' dropped successfully");
        }

        public override IExecutionOperator CopyWithNewSource(IExecutionOperator newSource)
            => new DropTableOperator(_tableName, _ifExists);
    }
}