namespace QoreDB.QueryEngine.Interfaces
{
    /// <summary>
    /// Represents the result of an executed query
    /// </summary>
    public interface IQueryResult
    {
        /// <summary>
        /// Materialize the query result
        /// </summary>
        /// <returns>A materialized version of the query</returns>
        public IQueryResult Materialize();
    }
}