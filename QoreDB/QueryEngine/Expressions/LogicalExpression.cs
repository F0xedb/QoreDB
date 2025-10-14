using QoreDB.Common.Extensions;

namespace QoreDB.QueryEngine.Expressions
{
    /// <summary>
    /// Represents a logical expression that combines two other expressions, such as with an AND or OR operator
    /// </summary>
    public class LogicalExpression : IExpression
    {
        /// <summary>
        /// Gets the expression on the left side of the logical operator
        /// </summary>
        public IExpression Left { get; }

        /// <summary>
        /// Gets the logical operator used in the expression
        /// </summary>
        public OperatorType Operator { get; }

        /// <summary>
        /// Gets the expression on the right side of the logical operator
        /// </summary>
        public IExpression Right { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalExpression"/> class
        /// </summary>
        /// <param name="left">The expression on the left side of the operator</param>
        /// <param name="op">The logical operator to apply</param>
        /// <param name="right">The expression on the right side of the operator</param>
        public LogicalExpression(IExpression left, OperatorType op, IExpression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        /// <summary>
        /// Returns a string representation of the logical expression
        /// </summary>
        public override string ToString()
        {
            return $"({Left} {Operator.GetString()} {Right})";
        }
        
        public IExpression Accept(IExpressionVisitor visitor)
        {
            return visitor.VisitLogical(this);
        }
    }
}