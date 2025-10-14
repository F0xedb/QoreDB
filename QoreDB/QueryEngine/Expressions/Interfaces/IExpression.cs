namespace QoreDB.QueryEngine.Expressions
{
    /// <summary>
    /// Represents a node in an expression tree, used in WHERE clauses
    /// </summary>
    public interface IExpression
    {
        /// <summary>
        /// Accepts a visitor, allowing it to process this expression node.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <returns>The potentially modified expression.</returns>
        IExpression Accept(IExpressionVisitor visitor);
    }
}
