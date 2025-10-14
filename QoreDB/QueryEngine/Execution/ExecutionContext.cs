using QoreDB.Catalog.Interfaces;
using QoreDB.QueryEngine.Interfaces;

namespace QoreDB.QueryEngine.Execution
{
    /// <summary>
    /// A concrete implementation of the execution context
    /// </summary>
    public class ExecutionContext : IExecutionContext
    {
        public ICatalogManager Catalog { get; }

        public ExecutionContext(ICatalogManager catalog)
        {
            Catalog = catalog;
        }
    }
}