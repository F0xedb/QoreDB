namespace QoreDB.QueryEngine.Expressions
{
    /// <summary>
    /// Represents a literal (constant) value in an expression, like a number or a string
    /// </summary>
    public class LiteralValue : IExpression
    {
        /// <summary>
        /// The constant value
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralValue"/> class
        /// </summary>
        /// <param name="val">The literal value</param>
        public LiteralValue(object val) { Value = val; }

        /// <summary>
        /// Returns a string representation of the literal value
        /// </summary>
        public override string ToString()
        {
            if (Value is string s)
            {
                return $"'{s}'"; // Add quotes around string literals
            }
            return Value?.ToString() ?? "NULL";
        }

        public IExpression Accept(IExpressionVisitor visitor)
        {
            return visitor.VisitLiteral(this);
        }
    }
}