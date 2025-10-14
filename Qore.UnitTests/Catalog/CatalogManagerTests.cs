using FluentAssertions;
using QoreDB.Catalog;
using QoreDB.Catalog.Models;
using QoreDB.StorageEngine.Index;
using QoreDB.StorageEngine.Index.Serializer;
using QoreDB.StorageEngine.Index.Serializer.Implementations;
using QoreDB.StorageEngine.Pager;
using System;
using System.Collections.Generic;

namespace Qore.UnitTests.Catalog
{
    [TestFixture]
    public class CatalogManagerTests
    {
        private InMemoryQorePager _pager;
        private BPlusTree<string, TableInfo> _tablesTree;
        private BPlusTree<string, ColumnInfo> _columnsTree;
        private CatalogManager _catalogManager;

        [SetUp]
        public void SetUp()
        {
            _pager = new InMemoryQorePager();

            _tablesTree = new BPlusTree<string, TableInfo>(1, 3);
            _columnsTree = new BPlusTree<string, ColumnInfo>(2, 3);

            _catalogManager = new CatalogManager(_pager);
        }

        [TearDown]
        public void TearDown()
        {
            _pager.Dispose();
        }

        private static List<ColumnInfo> GetSampleColumns() => new()
        {
            new ColumnInfo("Id", typeof(int)),
            new ColumnInfo("Name", typeof(string))
        };

        [Test]
        public void CreateTable_WhenTableDoesNotExist_ShouldSucceed()
        {
            // Arrange
            var tableName = "Users";
            var columns = GetSampleColumns();

            // Act
            _catalogManager.CreateTable(tableName, columns);
            var tableInfo = _catalogManager.GetTable(tableName);

            // Assert
            tableInfo.Should().NotBeNull();
            tableInfo.TableName.Should().Be(tableName);
            tableInfo.Columns.Should().HaveCount(2);
            tableInfo.Columns[0].ColumnName.Should().Be("Id");
            tableInfo.Columns[1].DataType.Should().Be(typeof(string));
        }

        [Test]
        public void CreateTable_WhenTableAlreadyExists_ShouldThrowException()
        {
            // Arrange
            var tableName = "Users";
            var columns = GetSampleColumns();
            _catalogManager.CreateTable(tableName, columns);

            // Act
            Action act = () => _catalogManager.CreateTable(tableName, columns);

            // Assert
            act.Should().Throw<Exception>().WithMessage($"Table '{tableName}' already exists");
        }

        [Test]
        public void GetTable_WhenTableExists_ShouldReturnTableInfo()
        {
            // Arrange
            var tableName = "Products";
            var columns = new List<ColumnInfo> { new("SKU", typeof(string)) };
            _catalogManager.CreateTable(tableName, columns);

            // Act
            var tableInfo = _catalogManager.GetTable(tableName);

            // Assert
            tableInfo.Should().NotBeNull();
            tableInfo.TableName.Should().Be(tableName);
        }

        [Test]
        public void GetTable_WhenTableDoesNotExist_ShouldReturnNull()
        {
            // Act
            var tableInfo = _catalogManager.GetTable("NonExistentTable");

            // Assert
            tableInfo.Should().BeNull();
        }

        [Test]
        [Ignore("Dropping tables is currently unsupported")]
        public void DropTable_WhenTableExists_ShouldRemoveTableAndColumns()
        {
            // Arrange
            var tableName = "Users";
            var columns = GetSampleColumns();
            _catalogManager.CreateTable(tableName, columns);

            // Act
            _catalogManager.DropTable(tableName, false);
            var tableInfo = _catalogManager.GetTable(tableName);

            // Assert
            tableInfo.Should().BeNull("because the table should have been dropped");
        }

        [Test]
        public void DropTable_WhenTableDoesNotExist_ShouldThrowException()
        {
            // Act
            Action act = () => _catalogManager.DropTable("NonExistentTable", false);

            // Assert
            act.Should().Throw<Exception>().WithMessage("Table 'NonExistentTable' not found");
        }

        [Test]
        public void Insert_WhenTableExistsAndDataIsValid_ShouldSucceed()
        {
            // Arrange
            var tableName = "Users";
            _catalogManager.CreateTable(tableName, GetSampleColumns());
            var row = new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "Alice" }
            };

            // Act
            Action act = () => _catalogManager.Insert<int>(tableName, row);

            // Assert
            act.Should().NotThrow();
        }

        [Test]
        public void Insert_WhenTableDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var row = new Dictionary<string, object> { { "Id", 1 } };

            // Act
            Action act = () => _catalogManager.Insert<int>("NonExistentTable", row);

            // Assert
            act.Should().Throw<Exception>().WithMessage("Table 'NonExistentTable' not found");
        }

        [Test]
        public void Insert_WhenPrimaryKeyIsMissing_ShouldThrowArgumentException()
        {
            // Arrange
            var tableName = "Users";
            _catalogManager.CreateTable(tableName, GetSampleColumns());
            var row = new Dictionary<string, object>
            {
                // Missing "Id"
                { "Name", "Bob" }
            };

            // Act
            Action act = () => _catalogManager.Insert<int>(tableName, row);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Primary key 'Id' not found in row data");
        }
    }
}