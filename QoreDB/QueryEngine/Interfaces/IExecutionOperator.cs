namespace QoreDB.QueryEngine.Interfaces
{
    /// <summary>
    /// Represents a single step (an operator) in a query execution plan
    /// Each operator can have a source from which it pulls data
    /// </summary>
    public interface IExecutionOperator
    {
        /// <summary>
        /// A user-friendly description of the operator and its parameters
        /// </summary>
        string Name { get; }

        /// <summary>
        /// How long it took to execute this <see cref="IExecutionOperator"/>
        /// </summary>
        TimeSpan ExecutionTime { get; }

        /// <summary>
        /// The operator that provides input to this operator, can be null for source operators
        /// </summary>
        IExecutionOperator Source { get; }

        /// <summary>
        /// Executes this step of the query plan
        /// </summary>
        /// <param name="context">The execution context containing shared resources like the catalog</param>
        /// <returns>The result of the execution</returns>
        IQueryResult Execute(IExecutionContext context);

        /// <summary>
        /// Creates a copy of the current operator but with a new source operator
        /// </summary>
        /// <param name="newSource">The new source operator to use</param>
        /// <returns>A new operator instance with the replaced source</returns>
        IExecutionOperator CopyWithNewSource(IExecutionOperator newSource);
    }
}