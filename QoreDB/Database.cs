using QoreDB.Catalog;
using QoreDB.Catalog.Interfaces;
using QoreDB.Catalog.Models;
using QoreDB.StorageEngine;
using QoreDB.StorageEngine.Index;
using QoreDB.StorageEngine.Index.Interfaces;
using QoreDB.StorageEngine.Index.Serializer;
using QoreDB.StorageEngine.Index.Serializer.Implementations;
using QoreDB.StorageEngine.Pager;
using QoreDB.StorageEngine.Pager.Interfaces;
using QoreDB.QueryEngine.Interfaces;
using QoreDB.QueryEngine.Execution;
using QoreDB.QueryEngine.Parser;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.QueryEngine.Optimizer;

namespace QoreDB
{
    /// <summary>
    /// Represents the main database entry point
    /// </summary>
    /// <remarks>
    /// This class provides methods for executing SQL queries and managing the database
    /// </remarks>
    public sealed class Database
    {
        /// <summary>
        /// The catalog manager for the database, used to manage tables and columns
        /// </summary>
        protected ICatalogManager Catalog { get; }

        /// <summary>
        /// The pager used for accessing the database file
        /// </summary>
        protected IPager Pager { get; }

        /// <summary>
        /// The parser responsible for converting raw SQL strings into an executable query plan
        /// </summary>
        protected ISqlParser Parser { get; }

        /// <summary>
        /// The executor responsible for running a query plan against the database
        /// </summary>
        protected IQueryExecutor Executor { get; }

        /// <summary>
        /// If we should capture developer information when executing queries against the database
        /// </summary>
        public bool DeveloperMode { get; set; } = false;

        /// <summary>
        /// If we should optimize the query execution plan
        /// </summary>
        public bool QueryOptimizationMode { get; set; } = true;


        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class using a file-based pager
        /// </summary>
        /// <param name="dbFilePath">The path to the database file</param>
        public Database(string dbFilePath)
            // TODO: Have a configuration file that allows configuration of db page sizes, in memory cache size, flush speed, ...
            : this(new QorePager(dbFilePath: dbFilePath))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class using an in-memory pager for transient storage
        /// </summary>
        public Database()
            : this(new InMemoryQorePager())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class using an in-memory pager for transient storage in debug mode
        /// </summary>
        /// <param name="degree"></param>
        /// <param name="pageSize"></param>
        /// <param name="cacheSize"></param>
        public Database(int degree, int pageSize, int cacheSize)
            : this(new InMemoryQorePager(null, pageSize, cacheSize), degree)
        {
            DeveloperMode = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class with a specific pager implementation
        /// </summary>
        /// <param name="pager">The pager to use for database storage</param>
        private Database(IPager pager, int degree = Constants.DEFAULT_BTREE_PAGE_DEGREE)
        {
            Pager = pager;

            Catalog = new CatalogManager(Pager);

            Parser = new SQLParser();
            Executor = new QueryExecutor(Catalog);
        }

        /// <summary>
        /// Executes a SQL query against the database
        /// </summary>
        /// <param name="sql">The SQL query string to execute</param>
        /// <returns>An <see cref="IQueryResult"/> containing the result of the query</returns>
        public IQueryResult Execute(string sql, int? limit = null)
        {
            var plan = Parser.Parse(sql);
            
            // If a take operator already exist then the user explicitly requested more than limit
            if (limit != null && !plan.ContainsType(typeof(TakeOperator)))
            {
                var filter = new TakeOperator(plan.Root, limit.Value, 0);
                plan = new QueryExecutionPlan(filter);
            }

            if (QueryOptimizationMode)
                plan = new QueryOptimizer().Optimize(plan);

            var result = Executor.Execute(plan);

            if (DeveloperMode)
                return new DebugQueryResult(result, plan.Root, Pager);

            return result;
        }

        public IBPlusTree<TKey, byte[]> GetTableTree<TKey>(string tableName)
            where TKey: IComparable<TKey>
        {
            if (!DeveloperMode)
                throw new InvalidOperationException("Can't fetch internal state when developer mode is turned off");

            return Catalog.GetTableTree<TKey>(tableName, out var _);
        }

        /// <inheritdoc cref="ICatalogManager.GetAllTables()" />
        public IEnumerable<TableInfo> GetTables()
            => Catalog.GetAllTables();

        /// <inheritdoc cref="ICatalogManager.GetTable(string)" />
        public TableInfo? GetTable(string tableName)
            => Catalog.GetTable(tableName);

        /// <inheritdoc cref="IPager.FileSize" />
        public long GetStorageFileSize
            => Pager.FileSize;
    }
}