using QoreDB.QueryEngine.Interfaces;
using System.Collections.Generic;

namespace QoreDB.QueryEngine.Execution.Models
{
    /// <summary>
    /// A query result that contains a collection of rows
    /// </summary>
    public class RowsQueryResult : IQueryResult
    {
        /// <summary>
        /// The collection of rows returned by the query
        /// </summary>
        public IEnumerable<IDictionary<string, object>> Rows { get; }

        public RowsQueryResult(IEnumerable<IDictionary<string, object>> rows)
        {
            Rows = rows;
        }

        public IQueryResult Materialize()
            => new RowsQueryResult(Rows.ToList());
    }
}