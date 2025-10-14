using QoreDB.Common.Extensions;

namespace QoreDB.QueryEngine.Expressions
{
    /// <summary>
    /// Represents an expression with a left and right side and an operator, such as (price > 50)
    /// </summary>
    public class BinaryExpression : IExpression
    {
        /// <summary>
        /// The left side of the expression, typically a <see cref="ColumnValue"/>
        /// </summary>
        public IExpression Left { get; }

        /// <summary>
        /// The comparison operator
        /// </summary>
        public OperatorType Operator { get; }

        /// <summary>
        /// The right side of the expression, typically a <see cref="LiteralValue"/>
        /// </summary>
        public IExpression Right { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryExpression"/> class
        /// </summary>
        /// <param name="left">The left side of the expression</param>
        /// <param name="op">The comparison operator</param>
        /// <param name="right">The right side of the expression</param>
        public BinaryExpression(IExpression left, OperatorType op, IExpression right)
        {
            Left = left; Operator = op; Right = right;
        }

        /// <summary>
        /// Returns a string representation of the binary expression
        /// </summary>
        public override string ToString()
        {
            return $"({Left} {Operator.GetString()} {Right})";
        }

        public IExpression Accept(IExpressionVisitor visitor)
        {
            return visitor.VisitBinary(this);
        }
    }
}