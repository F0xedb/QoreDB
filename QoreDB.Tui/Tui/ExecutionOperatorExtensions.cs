using QoreDB.QueryEngine.Interfaces;
using System;
using System.Collections.Generic;

namespace QoreDB.Tui.Tui
{
    /// <summary>
    /// Provides extension methods for the IExecutionOperator interface to simplify
    /// traversal and analysis of the execution plan tree.
    /// </summary>
    public static class ExecutionOperatorExtensions
    {
        /// <summary>
        /// Calculates the time delta of an operator, which is its own execution time
        /// minus the execution time of its direct source operator.
        /// </summary>
        public static TimeSpan GetDelta(this IExecutionOperator op)
            => op.ExecutionTime - (op.Source?.ExecutionTime ?? TimeSpan.Zero);

        /// <summary>
        /// Calculates the depth of an operator in the execution tree.
        /// </summary>
        public static int GetDepth(this IExecutionOperator op, int currentDepth = 0)
            => op.Source == null ? currentDepth : GetDepth(op.Source, currentDepth + 1);

        /// <summary>
        /// Flattens the execution plan tree into a list of operators, starting from the root.
        /// The execution plan is a linked list, so this method traverses it and reverses
        /// the result to get the correct top-down order.
        /// </summary>
        public static List<IExecutionOperator> Flatten(this IExecutionOperator op)
        {
            var list = new List<IExecutionOperator>();
            var current = op;
            while (current != null)
            {
                list.Add(current);
                current = current.Source;
            }
            // The plan is traversed from leaf to root, so we reverse to get root-to-leaf
            list.Reverse();
            return list;
        }
    }
}