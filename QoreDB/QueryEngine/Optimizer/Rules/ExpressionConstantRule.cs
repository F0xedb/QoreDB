using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.QueryEngine.Expressions;
using QoreDB.QueryEngine.Interfaces;
using QoreDB.QueryEngine.Optimizer.Visitors;

namespace QoreDB.QueryEngine.Optimizer.Rules
{
    /// <summary>
    /// An optimization rule that evaluates constant expressions in filter predicates
    /// </summary>
    public class ExpressionConstantRule : IOptimizerRule
    {
        /// <summary>
        /// Applies the constant expression evaluation optimization to the query plan
        /// </summary>
        /// <param name="plan">The execution plan to optimize</param>
        /// <returns>An optimized execution plan with constant expressions evaluated</returns>
        public IExecutionOperator Apply(IExecutionOperator plan)
        {
            if (plan is not FilterOperator filter)
            {
                if (plan.Source != null)
                {
                    var newSource = Apply(plan.Source);
                    if (newSource != plan.Source)
                    {
                        return plan.CopyWithNewSource(newSource);
                    }
                }
                return plan;
            }

            var visitor = new ConstantFoldingExpressionVisitor();
            var newPredicate = visitor.Visit(filter.Predicate);

            if (newPredicate is LiteralValue literal)
            {
                if (literal.Value is bool b && b)
                {
                    return Apply(filter.Source);
                }
            }
            
            var optimizedSource = Apply(filter.Source);

            if (optimizedSource != filter.Source || newPredicate != filter.Predicate)
            {
                return new FilterOperator(optimizedSource, newPredicate);
            }

            return filter;
        }
    }
}