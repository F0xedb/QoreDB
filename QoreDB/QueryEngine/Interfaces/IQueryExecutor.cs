using QoreDB.QueryEngine.Execution;

namespace QoreDB.QueryEngine.Interfaces
{
    /// <summary>
    /// Defines a component that executes a query plan against the database
    /// </summary>
    public interface IQueryExecutor
    {
        /// <summary>
        /// Executes a query plan
        /// </summary>
        /// <param name="plan">The query plan to execute</param>
        /// <returns>The result of the query execution</returns>
        IQueryResult Execute(QueryExecutionPlan plan);
    }
}