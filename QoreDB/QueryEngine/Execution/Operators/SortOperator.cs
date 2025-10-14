using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QoreDB.QueryEngine.Execution.Operators
{
    /// <summary>
    /// An operator that sorts rows from its source based on a column
    /// </summary>
    public class SortOperator : BaseExecutionOperator
    {
        public override string Name => $"Sort(By={SortColumnName}, Asc={_isAscending})";

        /// <summary>
        /// The operator that provides the input rows to be sorted
        /// </summary>
        public override IExecutionOperator Source { get; }

        public readonly string SortColumnName;
        
        private readonly bool _isAscending;

        public SortOperator(IExecutionOperator source, string columnName, bool isAscending = true)
        {
            Source = source;
            SortColumnName = columnName;
            _isAscending = isAscending;
        }

        protected override IQueryResult ExecuteInternal(IExecutionContext context)
        {
            var inputResult = (RowsQueryResult)Source.Execute(context);

            var sortedRows = _isAscending
                ? inputResult.Rows.OrderBy(row => row[SortColumnName])
                : inputResult.Rows.OrderByDescending(row => row[SortColumnName]);

            return new RowsQueryResult(sortedRows);
        }

        public override IExecutionOperator CopyWithNewSource(IExecutionOperator newSource)
            => new SortOperator(newSource, SortColumnName, _isAscending);
    }
}