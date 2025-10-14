using QoreDB.QueryEngine.Execution;

namespace QoreDB.QueryEngine.Interfaces
{
    /// <summary>
    /// Defines a component that builds a query execution plan from a raw SQL string
    /// </summary>
    public interface ISqlParser
    {
        /// <summary>
        /// Parses a raw SQL query string into a query execution plan
        /// </summary>
        /// <param name="query">The SQL query string to parse</param>
        /// <returns>A logical query execution plan representing the parsed query</returns>
        QueryExecutionPlan Parse(string query);
    }
}