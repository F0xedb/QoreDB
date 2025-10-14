using QoreDB.QueryEngine.Interfaces;
using System;

namespace QoreDB.Tui.Tui.Printers
{
    /// <summary>
    /// Defines the contract for a strategy that prints a query result to the console.
    /// </summary>
    public interface IResultPrinter
    {
        /// <summary>
        /// Prints the specified query result.
        /// </summary>
        /// <param name="result">The query result to print.</param>
        /// <param name="duration">The total execution time for the query.</param>
        void Print(IQueryResult result, TimeSpan duration);
    }
}