using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;
using QoreDB.Tui.Tui.Printers;

namespace QoreDB.Tui.Tui
{
    /// <summary>
    /// A factory for creating the appropriate result printer based on the query result type.
    /// </summary>
    public static class QueryResultPrinterFactory
    {
        private static readonly StandardResultPrinter _standardPrinter = new();
        /// <summary>
        /// Gets the correct printer for the given query result.
        /// </summary>
        /// <param name="result">The query result.</param>
        /// <returns>An IResultPrinter that can handle the result.</returns>
        public static IResultPrinter GetPrinter(Database db, IQueryResult result)
        {
            if (result is DebugQueryResult)
                return new DebugResultPrinter(db, _standardPrinter);
            
            return _standardPrinter;
        }
    }
}