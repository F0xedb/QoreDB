using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace QoreDB.QueryEngine.Execution.Operators
{
    /// <summary>
    /// An operator that selects a subset of columns from its input
    /// </summary>
    public class ProjectionOperator : BaseExecutionOperator
    {
        public override string Name => $"Project({(Columns == null ? "*" : string.Join(", ", Columns))})";

        public override IExecutionOperator Source { get; }
        public readonly List<string> Columns;

        public ProjectionOperator(IExecutionOperator source, List<string> columns)
        {
            Source = source;
            Columns = columns;
        }

        protected override IQueryResult ExecuteInternal(IExecutionContext context)
        {
            var inputResult = (RowsQueryResult)Source.Execute(context);

            // _columns == null means we did a "SELECT *" e.g. specified all columns
            // So we'll skip projection
            if (Columns == null)
                return inputResult;

            return new RowsQueryResult(Project(inputResult.Rows));
        }

        private IEnumerable<IDictionary<string, object>> Project(IEnumerable<IDictionary<string, object>> rows)
        {
            foreach (var row in rows)
            {
                yield return new ProjectedRow(row, Columns);
            }
        }

        public override IExecutionOperator CopyWithNewSource(IExecutionOperator newSource)
            => new ProjectionOperator(newSource, Columns);
    }
}