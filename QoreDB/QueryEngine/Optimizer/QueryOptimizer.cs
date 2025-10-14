using QoreDB.QueryEngine.Execution;
using QoreDB.QueryEngine.Interfaces;
using QoreDB.QueryEngine.Optimizer.Rules;

namespace QoreDB.QueryEngine.Optimizer
{
    public class QueryOptimizer : IQueryOptimizer
    {
        /// <summary>
        /// The list of optimization rules to apply to the plan
        /// </summary>
        private readonly List<IOptimizerRule> _rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryOptimizer"/> class
        /// </summary>
        public QueryOptimizer()
        {
            _rules =
            [
                new ExpressionConstantRule(),
                new PredicatePushdownRule(),
                new ProjectionPushdownRule(),
            ];
        }

        public QueryExecutionPlan Optimize(QueryExecutionPlan plan)
        {
            var optimizedRoot = plan.Root;
            foreach (var rule in _rules)
            {
                optimizedRoot = rule.Apply(optimizedRoot);
            }
            return new QueryExecutionPlan(optimizedRoot);
        }

    }
}