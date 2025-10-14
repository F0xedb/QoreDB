using QoreDB.QueryEngine.Interfaces;

namespace QoreDB.QueryEngine.Optimizer.Rules
{
    /// <summary>
    /// Represents a rule that can be applied to an execution plan to optimize it
    /// </summary>
    public interface IOptimizerRule
    {
        /// <summary>
        /// Applies the optimization rule to a given execution plan
        /// </summary>
        /// <param name="plan">The execution plan to optimize</param>
        /// <returns>A new, optimized execution plan</returns>
        IExecutionOperator Apply(IExecutionOperator plan);
    }
}