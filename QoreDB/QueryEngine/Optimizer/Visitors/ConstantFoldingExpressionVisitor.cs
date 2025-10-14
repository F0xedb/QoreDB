using System.Collections.Generic;
using QoreDB.QueryEngine.Expressions;

namespace QoreDB.QueryEngine.Optimizer.Visitors
{
    /// <summary>
    /// A visitor that evaluates (folds) constant expressions into a single literal value.
    /// </summary>
    public class ConstantFoldingExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Visits a binary expression. If both sides are literals, it evaluates them 
        /// and returns a new literal with the result.
        /// </summary>
        /// <param name="binary">The binary expression to visit.</param>
        /// <returns>A new literal value if the expression can be folded, otherwise the original expression with potentially folded children.</returns>
        public override IExpression VisitBinary(BinaryExpression binary)
        {
            var left = Visit(binary.Left);
            var right = Visit(binary.Right);

            if (left is LiteralValue && right is LiteralValue)
            {
                var evaluator = new ExpressionEvaluator();
                var result = evaluator.Evaluate(new BinaryExpression(left, binary.Operator, right), new Dictionary<string, object>());
                return new LiteralValue(result);
            }

            return new BinaryExpression(left, binary.Operator, right);
        }

        /// <summary>
        /// Visits a logical expression. If both sides are literals, it evaluates them 
        /// and returns a new literal with the result.
        /// </summary>
        /// <param name="logical">The logical expression to visit.</param>
        /// <returns>A new literal value if the expression can be folded, otherwise the original expression with potentially folded children.</returns>
        public override IExpression VisitLogical(LogicalExpression logical)
        {
            var left = Visit(logical.Left);
            var right = Visit(logical.Right);

            if (left is LiteralValue l && right is LiteralValue r)
            {
                var evaluator = new ExpressionEvaluator();
                var result = evaluator.Evaluate(new LogicalExpression(l, logical.Operator, r), new Dictionary<string, object>());
                return new LiteralValue(result);
            }

            return new LogicalExpression(left, logical.Operator, right);
        }
    }
}