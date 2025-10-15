using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.QueryEngine.Interfaces;

namespace QoreDB.QueryEngine.Optimizer.Rules
{
    /// <summary>
    /// An optimization rule that pushes filter predicates as far down the execution tree as possible
    /// </summary>
    /// <remarks>
    /// This reduces the number of rows that need to be processed by upstream operators
    /// </remarks>
    public class PredicatePushdownRule : IOptimizerRule
    {
        /// <summary>
        /// Applies the predicate pushdown optimization to the query plan
        /// </summary>
        /// <param name="plan">The execution plan to optimize</param>
        /// <returns>An optimized execution plan with filters pushed down</returns>
        public IExecutionOperator Apply(IExecutionOperator plan)
        {
            // First, apply the rule to the source recursively (post-order traversal)
            if (plan.Source != null)
            {
                var newSource = Apply(plan.Source);
                plan = plan.CopyWithNewSource(newSource);
            }

            if (plan is FilterOperator filter)
            {
                // Now, check the current node and its newly-optimized child
                if (filter.Source is ProjectionOperator projection)
                {
                    // Swap the Filter and Projection operators
                    var newFilter = new FilterOperator(projection.Source, filter.Predicate);
                    return new ProjectionOperator(newFilter, projection.Columns);
                }

                if (filter.Source is SortOperator sort)
                {
                    // Swap the Filter and Sort operators
                    var newFilter = new FilterOperator(sort.Source, filter.Predicate);
                    return new SortOperator(newFilter, sort.SortColumnName);
                }
            }

            return plan;
        }
    }
}