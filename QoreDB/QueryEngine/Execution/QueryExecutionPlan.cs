using QoreDB.QueryEngine.Interfaces;

namespace QoreDB.QueryEngine.Execution
{
    /// <summary>
    /// Represents a complete, executable query plan as a tree of operators
    /// </summary>
    public class QueryExecutionPlan
    {
        /// <summary>
        /// The root operator of the execution tree
        /// </summary>
        public IExecutionOperator Root { get; }

        public QueryExecutionPlan(IExecutionOperator root)
        {
            Root = root;
        }
        
        /// <summary>
        /// Travers the plan to see if <paramref name="operatorType"/> exists within it
        /// </summary>
        /// <param name="operatorType">The type of the operator to check</param>
        /// <returns>True if the type was found</returns>
        public bool ContainsType(Type operatorType)
        {
            var executionOperator = Root;
            while (executionOperator != null) 
            {
                if (executionOperator.GetType().Equals(operatorType))
                    return true;

                executionOperator = executionOperator.Source;
            }

            return false;
        }
    }
}