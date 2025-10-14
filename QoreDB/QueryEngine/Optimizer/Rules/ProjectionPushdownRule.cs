// QoreDB/QueryEngine/Optimizer/Rules/ProjectionPushdownRule.cs

using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.QueryEngine.Expressions;
using QoreDB.QueryEngine.Optimizer.Visitors;
using QoreDB.QueryEngine.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace QoreDB.QueryEngine.Optimizer.Rules
{
    /// <summary>
    /// An optimization rule that pushes projections as far down the execution tree as possible
    /// </summary>
    /// <remarks>
    /// This reduces the amount of data that needs to be materialized and processed by intermediate operators
    /// </remarks>
    public class ProjectionPushdownRule : IOptimizerRule
    {
        /// <summary>
        /// Applies the projection pushdown optimization to the query plan
        /// </summary>
        /// <param name="plan">The execution plan to optimize</param>
        /// <returns>An optimized execution plan with projections pushed down</returns>
        public IExecutionOperator Apply(IExecutionOperator plan)
        {
            if (plan is not ProjectionOperator projection)
            {
                if (plan.Source != null)
                {
                    return plan.CopyWithNewSource(Apply(plan.Source));
                }
                return plan;
            }

            var requiredColumns = new HashSet<string>(projection.Columns ?? new List<string>());
            
            // Collect all columns required by operators below the current projection
            CollectRequiredColumns(projection.Source, requiredColumns);
            
            var newProjection = new ProjectionOperator(FindScan(projection.Source), requiredColumns.ToList());

            var newSource = RebuildWithNewSource(projection.Source, newProjection);

            return new ProjectionOperator(newSource, projection.Columns);
        }

        private void CollectRequiredColumns(IExecutionOperator plan, HashSet<string> columns)
        {
            if (plan == null) return;

            if (plan is FilterOperator filter)
            {
                var collector = new ColumnCollectorExpressionVisitor();
                collector.Visit(filter.Predicate);
                foreach (var col in collector.Columns)
                {
                    columns.Add(col);
                }
            }
            else if (plan is SortOperator sort)
            {
                columns.Add(sort.SortColumnName);
            }
            
            CollectRequiredColumns(plan.Source, columns);
        }

        private void CollectColumnsFromExpression(IExpression expression, HashSet<string> columns)
        {
            if (expression is BinaryExpression binary)
            {
                CollectColumnsFromExpression(binary.Left, columns);
                CollectColumnsFromExpression(binary.Right, columns);
            }
            else if (expression is ColumnValue column)
            {
                columns.Add(column.ColumnName);
            }
        }

        private IExecutionOperator FindScan(IExecutionOperator plan)
        {
            var current = plan;
            while (current != null && current is not TableScanOperator)
            {
                current = current.Source;
            }
            return current;
        }

        private IExecutionOperator RebuildWithNewSource(IExecutionOperator original, IExecutionOperator newScan)
        {
            if (original is TableScanOperator)
            {
                return newScan;
            }
            if (original.Source != null)
            {
                var newSource = RebuildWithNewSource(original.Source, newScan);
                return original.CopyWithNewSource(newSource);
            }
            return original;
        }
    }
}