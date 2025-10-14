namespace QoreDB.QueryEngine.Expressions
{
    /// <summary>
    /// Represents a reference to a column's value within a row
    /// </summary>
    public class ColumnValue : IExpression
    {
        /// <summary>
        /// The name of the column being referenced
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnValue"/> class
        /// </summary>
        /// <param name="name">The name of the column</param>
        public ColumnValue(string name) { ColumnName = name; }

        /// <summary>
        /// Returns the column name
        /// </summary>
        public override string ToString() => ColumnName;

        public IExpression Accept(IExpressionVisitor visitor)
        {
            return visitor.VisitColumn(this);
        }
    }
}