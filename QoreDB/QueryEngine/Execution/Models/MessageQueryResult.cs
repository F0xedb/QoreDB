using QoreDB.QueryEngine.Interfaces;

namespace QoreDB.QueryEngine.Execution.Models
{
    /// <summary>
    /// A query result that contains a status message
    /// </summary>
    public class MessageQueryResult : IQueryResult
    {
        /// <summary>
        /// The status message from the query execution, such as "1 row inserted"
        /// </summary>
        public string Message { get; }

        public MessageQueryResult(string message)
        {
            Message = message;
        }

        public IQueryResult Materialize()
            => this;
    }
}