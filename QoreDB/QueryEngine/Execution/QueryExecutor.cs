using QoreDB.Catalog.Interfaces;
using QoreDB.QueryEngine.Interfaces;
using QoreDB.QueryEngine.Optimizer;

namespace QoreDB.QueryEngine.Execution
{
    /// <summary>
    /// Executes a query plan by invoking the root operator
    /// </summary>
    public class QueryExecutor : IQueryExecutor
    {
        private readonly IExecutionContext _context;

        public QueryExecutor(ICatalogManager catalog)
        {
            _context = new ExecutionContext(catalog);
        }

        /// <summary>
        /// Executes a query plan
        /// </summary>
        /// <param name="plan">The query plan to execute</param>
        /// <returns>The result of the query execution</returns>
        public IQueryResult Execute(QueryExecutionPlan plan)
            => plan.Root.Execute(_context);
    }
}