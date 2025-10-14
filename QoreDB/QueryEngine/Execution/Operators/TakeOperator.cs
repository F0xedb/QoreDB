using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Expressions;
using QoreDB.QueryEngine.Interfaces;
using System.Collections.Generic;

namespace QoreDB.QueryEngine.Execution.Operators
{
    /// <summary>
    /// An operator that takes rows from its source
    /// </summary>
    public class TakeOperator : BaseExecutionOperator
    {
        public override string Name => $"Take({_amount})";

        /// <summary>
        /// The operator that provides the input rows to be filtered
        /// </summary>
        public override IExecutionOperator Source { get; }

        /// <summary>
        /// The amount of row to take from <see cref="Source"/>
        /// </summary>
        private readonly int _amount;

        /// <summary>
        /// The amount of row to skip from <see cref="Source"/> before taking values
        /// </summary>
        private readonly int _skip;

        public TakeOperator(IExecutionOperator source, int amount, int offset)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), $"Can't take {amount} of rows from the source");

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), $"Can't offset {offset} of rows from the source");


            Source = source;
            _amount = amount;
            _skip = offset;
        }

        protected override IQueryResult ExecuteInternal(IExecutionContext context)
        {
            var inputResult = (RowsQueryResult)Source.Execute(context);
            var filteredRows = inputResult.Rows
                    .Skip(_skip)
                    .Take(_amount);
            return new RowsQueryResult(filteredRows);
        }

        public override IExecutionOperator CopyWithNewSource(IExecutionOperator newSource)
            => new TakeOperator(newSource, _amount, _skip);
    }
}