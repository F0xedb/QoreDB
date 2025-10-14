using QoreDB.StorageEngine.Interfaces;
using QoreDB.StorageEngine.Pager.Interfaces;
using System;

namespace QoreDB.StorageEngine
{
    /// <summary>
    /// Manages the database header page (page 0), which stores pointers to system trees
    /// </summary>
    internal class DatabaseHeader : IDatabaseHeader
    {
        /// <summary>
        /// The page number of the root of the tables B+ Tree
        /// </summary>
        public int TablesRootPageId { get; private set; }

        /// <summary>
        /// The page number of the root of the columns B+ Tree
        /// </summary>
        public int ColumnsRootPageId { get; private set; }

        private const int TablesRootPageOffset = 0;
        private const int ColumnsRootPageOffset = 4;
        private const int HeaderPageId = 0;

        private DatabaseHeader(int tablesRoot, int columnsRoot)
        {
            TablesRootPageId = tablesRoot;
            ColumnsRootPageId = columnsRoot;
        }

        /// <summary>
        /// Loads the header from page 0 of the pager, or creates a new one if it doesn't exist
        /// </summary>
        /// <param name="pager">The pager for the database</param>
        /// <returns>An initialized DatabaseHeader instance</returns>
        public static DatabaseHeader Load(IPager pager)
        {
            if (pager.PageExists(HeaderPageId))
            {
                // Database already exists, so load the header page
                var headerPage = pager.GetPage(HeaderPageId);
                var tablesRoot = BitConverter.ToInt32(headerPage.Data, TablesRootPageOffset);
                var columnsRoot = BitConverter.ToInt32(headerPage.Data, ColumnsRootPageOffset);
                return new DatabaseHeader(tablesRoot, columnsRoot);
            }

            // This is a new database, so we must initialize it
            pager.AllocatePage(); // Allocate page 0 for the header

            var newTablesRoot = pager.AllocatePage();
            var newColumnsRoot = pager.AllocatePage();
            
            var header = new DatabaseHeader(newTablesRoot, newColumnsRoot);
            header.SaveChanges(pager);
            
            return header;
        }

        /// <summary>
        /// Writes the current header state to page 0 of the pager
        /// </summary>
        /// <param name="pager">The pager for the database</param>
        public void SaveChanges(IPager pager)
        {
            var headerPage = pager.GetPage(HeaderPageId);
            BitConverter.GetBytes(TablesRootPageId).CopyTo(headerPage.Data, TablesRootPageOffset);
            BitConverter.GetBytes(ColumnsRootPageId).CopyTo(headerPage.Data, ColumnsRootPageOffset);
            pager.WritePage(HeaderPageId, headerPage.Data);
        }
    }
}