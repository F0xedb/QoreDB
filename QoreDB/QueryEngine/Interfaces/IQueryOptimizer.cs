using  QoreDB.QueryEngine.Execution;

namespace QoreDB.QueryEngine.Interfaces {
    
    /// <summary>
    /// Applies a series of optimization rules to a query execution plan
    /// </summary>
    public interface IQueryOptimizer {

        /// <summary>
        /// Optimizes the given query execution plan by applying all registered rules
        /// </summary>
        /// <param name="plan">The plan to optimize</param>
        /// <returns>A new, optimized query execution plan</returns>
        QueryExecutionPlan Optimize(QueryExecutionPlan plan);
    }
}