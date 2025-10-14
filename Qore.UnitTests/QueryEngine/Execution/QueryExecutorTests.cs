using FluentAssertions;
using Moq;
using QoreDB.Catalog.Interfaces;
using QoreDB.Catalog.Models;
using QoreDB.QueryEngine.Execution;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.StorageEngine.Index.Interfaces;

namespace Qore.UnitTests.QueryEngine.Execution
{
    [TestFixture]
    public class QueryExecutorTests
    {
        private Mock<ICatalogManager> _mockCatalog;
        private QueryExecutor _executor;

        [SetUp]
        public void SetUp()
        {
            _mockCatalog = new Mock<ICatalogManager>();
            _executor = new QueryExecutor(_mockCatalog.Object);
        }

        [Test]
        public void Execute_WithInsertPlan_CallsCatalogInsertAndReturnsMessage()
        {
            // Arrange
            var tableName = "Users";
            var row = new Dictionary<string, object> { { "Id", 1 }, { "Name", "Test" } };
            var plan = new QueryExecutionPlan(new InsertOperator(tableName, row));

            var tableInfo = new TableInfo(tableName, new List<ColumnInfo> { new("Id", typeof(int)) }, 1);
            _mockCatalog.Setup(c => c.GetTable(tableName)).Returns(tableInfo);

            // Act
            var result = _executor.Execute(plan) as MessageQueryResult;

            // Assert
            _mockCatalog.Verify(c => c.Insert<int>(tableName, row), Times.Once);
            result.Should().NotBeNull();
            result.Message.Should().Be("1 row inserted");
        }

        [Test]
        public void Execute_WithSelectPlan_ReturnsRowsResult()
        {
            // Arrange
            var tableName = "Users";
            var plan = new QueryExecutionPlan(
                new ProjectionOperator(
                    new TableScanOperator(tableName),
                    new List<string> { "Id" }
                )
            );

            var tableInfo = new TableInfo(tableName, new List<ColumnInfo> { new("Id", typeof(int)) }, 1);
            _mockCatalog.Setup(c => c.GetTable(tableName)).Returns(tableInfo);

            // THE FIX: We must mock the call to GetTableTree that the TableScanOperator makes.
            var mockTree = new Mock<IBPlusTree<int, byte[]>>();
            mockTree.Setup(t => t.GetAllValues()).Returns(new List<byte[]>()); // Return an empty list of values
            _mockCatalog.Setup(c => c.GetTableTree<int>(tableName, out tableInfo)).Returns(mockTree.Object);

            // Act
            var result = _executor.Execute(plan) as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().BeEmpty();
        }

        [Test]
        public void Execute_WithPlanThatThrows_ExceptionPropagates()
        {
            // Arrange
            var plan = new QueryExecutionPlan(new TableScanOperator("BadTable"));
            _mockCatalog.Setup(c => c.GetTable("BadTable")).Returns((TableInfo)null);

            // Act
            Action act = () => _executor.Execute(plan).Materialize();

            // Assert
            act.Should().Throw<Exception>().WithMessage("Table 'BadTable' not found*");
        }

        [Test]
        public void Execute_WithCreateTablePlan_CallsCatalogCreateTable()
        {
            // Arrange
            var tableName = "NewEmployees";
            var columns = new List<ColumnInfo> { new("Id", typeof(int)) };
            var plan = new QueryExecutionPlan(new CreateTableOperator(tableName, columns));

            // Act
            var result = _executor.Execute(plan) as MessageQueryResult;

            // Assert
            _mockCatalog.Verify(c => c.CreateTable(tableName, columns), Times.Once);
            result.Should().NotBeNull();
            result.Message.Should().Be($"Table '{tableName}' created successfully");
        }

        [Test]
        public void Execute_WithDropTablePlan_CallsCatalogDropTable()
        {
            // Arrange
            var tableName = "OldProducts";
            var plan = new QueryExecutionPlan(new DropTableOperator(tableName, false));

            // Act
            var result = _executor.Execute(plan) as MessageQueryResult;

            // Assert
            _mockCatalog.Verify(c => c.DropTable(tableName, false), Times.Once);
            result.Should().NotBeNull();
            result.Message.Should().Be($"Table '{tableName}' dropped successfully");
        }

        [Test]
        public void ContainsType_OperatorWithType_ContainsType()
        {
            // Arrange
            var tso = new TableScanOperator("BadTable");
            var to = new TakeOperator(tso, 1, 1);
            var plan = new QueryExecutionPlan(to);

            // Act
            var toResult = plan.ContainsType(typeof(TakeOperator));
            var tsoResult = plan.ContainsType(typeof(TableScanOperator));
            var doResult = plan.ContainsType(typeof(DropTableOperator));

            // Assert
            toResult.Should().BeTrue();
            tsoResult.Should().BeTrue();
            doResult.Should().BeFalse();
        }
    }
}