using System.Collections.Generic;
using QoreDB.QueryEngine.Expressions;

namespace QoreDB.QueryEngine.Optimizer.Visitors
{
    /// <summary>
    /// A visitor that traverses an expression tree and collects the names of all columns referenced.
    /// </summary>
    public class ColumnCollectorExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Gets the list of column names found in the expression tree.
        /// </summary>
        public List<string> Columns { get; } = new List<string>();

        /// <summary>
        /// Visits a column value node and adds its name to the list of columns.
        /// </summary>
        /// <param name="column">The column value to visit.</param>
        /// <returns>The original column value expression.</returns>
        public override IExpression VisitColumn(ColumnValue column)
        {
            if (!Columns.Contains(column.ColumnName))
            {
                Columns.Add(column.ColumnName);
            }
            return column;
        }
    }
}