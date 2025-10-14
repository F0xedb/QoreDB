using QoreDB.QueryEngine.Interfaces;
using QoreDB.StorageEngine.Pager.Interfaces;

namespace QoreDB.QueryEngine.Execution.Models
{
    /// <summary>
    /// A query result that wraps another result and includes diagnostic information
    /// </summary>
    public class DebugQueryResult : IQueryResult
    {
        /// <summary>
        /// The original result of the query (e.g., rows or a message)
        /// </summary>
        public IQueryResult OriginalResult { get; }

        /// <summary>
        /// The root of the query execution plan tree
        /// </summary>
        public IExecutionOperator ExecutionPlan { get; }

        public IPager Pager { get; }

        public long FileSize { get; }

        public long PageCount { get; }

        public long CachedPages { get; }

        public DebugQueryResult(IQueryResult originalResult, IExecutionOperator executionPlan, IPager pager)
        {
            OriginalResult = originalResult;
            ExecutionPlan = executionPlan;
            Pager = pager;

            FileSize = pager.FileSize;
            PageCount = pager.AllocatedPages;

            if (pager is ICachedPager cachedPager)
                CachedPages = cachedPager.CachedPages;
        }

        public IQueryResult Materialize()
            => new DebugQueryResult(OriginalResult.Materialize(), ExecutionPlan, Pager);
    }
}