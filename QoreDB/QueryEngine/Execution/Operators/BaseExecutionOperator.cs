using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;
using System;
using System.Diagnostics;

namespace QoreDB.QueryEngine.Execution.Operators
{
    public abstract class BaseExecutionOperator : IExecutionOperator
    {
        public abstract string Name { get; }
        public abstract IExecutionOperator Source { get; }
        public TimeSpan ExecutionTime { get; private set; }

        public IQueryResult Execute(IExecutionContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = ExecuteInternal(context);

            stopwatch.Stop();

            if (result is RowsQueryResult rowsResult)
            {
                var setupTime = stopwatch.Elapsed;
                ExecutionTime = setupTime; // Start with setup time.

                var timedEnumerable = new TimedEnumerable(rowsResult.Rows,
                    elapsed => ExecutionTime = setupTime + elapsed); // Add enumeration time to setup time

                return new RowsQueryResult(timedEnumerable);
            }
            else
            {
                // For non-row-returning operators, the time is just the execution time.
                ExecutionTime = stopwatch.Elapsed;
                return result;
            }
        }

        protected abstract IQueryResult ExecuteInternal(IExecutionContext context);

        public abstract IExecutionOperator CopyWithNewSource(IExecutionOperator newSource);
    }
}