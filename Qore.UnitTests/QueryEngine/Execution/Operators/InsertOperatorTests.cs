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
    public class InsertOperatorTests
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
        public void Execute_WhenTableExists_CallsCatalogInsertAndReturnsMessage()
        {
            // Arrange
            var tableName = "Users";
            var row = new Dictionary<string, object> { { "Id", 1 } };
            var op = new InsertOperator(tableName, row);

            var tableInfo = new TableInfo(tableName, new List<ColumnInfo> { new("Id", typeof(int)) }, 1);
            _mockCatalog.Setup(c => c.GetTable(tableName)).Returns(tableInfo);

            // Act
            var result = op.Execute(_context) as MessageQueryResult;

            // Assert
            _mockCatalog.Verify(c => c.Insert<int>(tableName, row), Times.Once);
            result.Should().NotBeNull();
            result.Message.Should().Be("1 row inserted");
        }

        [Test]
        public void Execute_WhenTableDoesNotExist_ThrowsException()
        {
            // Arrange
            var op = new InsertOperator("BadTable", new Dictionary<string, object>());
            _mockCatalog.Setup(c => c.GetTable("BadTable")).Returns((TableInfo)null);

            // Act
            Action act = () => op.Execute(_context);

            // Assert
            act.Should().Throw<Exception>().WithMessage("Table 'BadTable' not found");
        }
    }
}