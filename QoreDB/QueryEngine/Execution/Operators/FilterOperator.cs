using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Expressions;
using QoreDB.QueryEngine.Interfaces;
using System.Collections.Generic;

namespace QoreDB.QueryEngine.Execution.Operators
{
    /// <summary>
    /// An operator that filters rows from its source based on a predicate
    /// </summary>
    public class FilterOperator : BaseExecutionOperator
    {
        public override string Name => $"Filter(Predicate={Predicate})"; // A proper ToString on the expression is needed for a full implementation

        /// <summary>
        /// The operator that provides the input rows to be filtered
        /// </summary>
        public override IExecutionOperator Source { get; }

        /// <summary>
        /// The expression to evaluate for each row
        /// </summary>
        public readonly IExpression Predicate;

        public FilterOperator(IExecutionOperator source, IExpression predicate)
        {
            Source = source;
            Predicate = predicate;
        }

        protected override IQueryResult ExecuteInternal(IExecutionContext context)
        {
            var inputResult = (RowsQueryResult)Source.Execute(context);
            return new RowsQueryResult(Filter(inputResult.Rows));
        }

        private IEnumerable<IDictionary<string, object>> Filter(IEnumerable<IDictionary<string, object>> rows)
        {
            var evaluator = new ExpressionEvaluator();

            foreach (var row in rows)
            {
                var result = evaluator.Evaluate(Predicate, row);
                if (result is bool b && b)
                {
                    yield return row;
                }
            }
        }

        public override IExecutionOperator CopyWithNewSource(IExecutionOperator newSource)
            => new FilterOperator(newSource, Predicate);
    }
}