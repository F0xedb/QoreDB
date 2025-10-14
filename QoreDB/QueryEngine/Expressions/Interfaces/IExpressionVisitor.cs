namespace QoreDB.QueryEngine.Expressions
{
    /// <summary>
    /// Defines a visitor for traversing an expression tree.
    /// </summary>
    public interface IExpressionVisitor
    {
        /// <summary>
        /// Visits the given expression node.
        /// </summary>
        /// <param name="expression">The expression to visit.</param>
        /// <returns>The modified expression.</returns>
        IExpression Visit(IExpression expression);

        /// <summary>
        /// Visits a binary expression node.
        /// </summary>
        /// <param name="binary">The binary expression to visit.</param>
        /// <returns>The modified expression.</returns>
        IExpression VisitBinary(BinaryExpression binary);

        /// <summary>
        /// Visits a column value node.
        /// </summary>
        /// <param name="column">The column value to visit.</param>
        /// <returns>The modified expression.</returns>
        IExpression VisitColumn(ColumnValue column);

        /// <summary>
        /// Visits a literal value node.
        /// </summary>
        /// <param name="literal">The literal value to visit.</param>
        /// <returns>The modified expression.</returns>
        IExpression VisitLiteral(LiteralValue literal);

        /// <summary>
        /// Visits a logical expression node.
        /// </summary>
        /// <param name="logical">The logical expression to visit.</param>
        /// <returns>The modified expression.</returns>
        IExpression VisitLogical(LogicalExpression logical);
    }
}