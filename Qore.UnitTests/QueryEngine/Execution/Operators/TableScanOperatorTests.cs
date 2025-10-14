using FluentAssertions;
using Moq;
using QoreDB.Catalog.Interfaces;
using QoreDB.Catalog.Models;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Execution.Operators;
using ExecutionContext = QoreDB.QueryEngine.Execution.ExecutionContext;


namespace Qore.UnitTests.QueryEngine.Execution.Operators
{
    [TestFixture]
    public class TableScanOperatorTests
    {
        private Mock<ICatalogManager> _mockCatalog;
        private ExecutionContext _context;

        [SetUp]
        public void SetUp()
        {
            _mockCatalog = new Mock<ICatalogManager>();
            _context = new ExecutionContext(_mockCatalog.Object);
        }

        [Test]
        public void Execute_WhenTableExists_ReturnsRowsResult()
        {
            // Arrange
            var tableName = "Users";
            var op = new TableScanOperator(tableName);
            var tableInfo = new TableInfo(tableName, new List<ColumnInfo>(), 1);
            _mockCatalog.Setup(c => c.GetTable(tableName)).Returns(tableInfo);

            // Act
            var result = op.Execute(_context) as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().BeEmpty(); // The mock scan returns no data for now
        }

        [Test]
        public void Execute_WhenTableDoesNotExist_ThrowsException()
        {
            // Arrange
            var op = new TableScanOperator("BadTable");
            _mockCatalog.Setup(c => c.GetTable("BadTable")).Returns((TableInfo)null);

            // Act
            Action act = () =>
            {
                var result = (RowsQueryResult)op.Execute(_context);
                result.Rows.ToList(); // Materialize 
            };

            // Assert
            act.Should().Throw<Exception>().WithMessage("Table 'BadTable' not found*");
        }
    }
}