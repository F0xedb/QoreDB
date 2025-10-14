namespace QoreDB.QueryEngine.Expressions
{
    public abstract class ExpressionVisitor : IExpressionVisitor
    {
        public virtual IExpression Visit(IExpression expression)
        {
            return expression switch
            {
                BinaryExpression binary => VisitBinary(binary),
                ColumnValue column => VisitColumn(column),
                LiteralValue literal => VisitLiteral(literal),
                LogicalExpression logical => VisitLogical(logical),
                _ => expression
            };
        }

        public virtual IExpression VisitBinary(BinaryExpression binary)
        {
            var left = Visit(binary.Left);
            var right = Visit(binary.Right);
            return new BinaryExpression(left, binary.Operator, right);
        }

        public virtual IExpression VisitColumn(ColumnValue column) => column;

        public virtual IExpression VisitLiteral(LiteralValue literal) => literal;

        public virtual IExpression VisitLogical(LogicalExpression logical)
        {
            var left = Visit(logical.Left);
            var right = Visit(logical.Right);
            return new LogicalExpression(left, logical.Operator, right);
        }
    }
}