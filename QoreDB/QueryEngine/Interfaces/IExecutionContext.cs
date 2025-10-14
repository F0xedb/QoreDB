using QoreDB.Catalog.Interfaces;

namespace QoreDB.QueryEngine.Interfaces
{
    /// <summary>
    /// Provides the context and resources needed for query execution
    /// </summary>
    public interface IExecutionContext
    {
        /// <summary>
        /// The catalog manager for accessing schema information
        /// </summary>
        ICatalogManager Catalog { get; }
    }
}