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
    public class CreateTableOperatorTests
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
        public void Execute_WhenCalled_CallsCatalogCreateTable()
        {
            // Arrange
            var tableName = "NewUsers";
            var columns = new List<ColumnInfo> { new("Id", typeof(int)) };
            var op = new CreateTableOperator(tableName, columns);

            // Act
            var result = op.Execute(_context) as MessageQueryResult;

            // Assert
            _mockCatalog.Verify(c => c.CreateTable(tableName, columns), Times.Once);
            result.Should().NotBeNull();
            result.Message.Should().Be($"Table '{tableName}' created successfully");
        }

        [Test]
        public void Execute_WhenCatalogThrows_ExceptionPropagates()
        {
            // Arrange
            var tableName = "ExistingTable";
            var columns = new List<ColumnInfo>();
            var op = new CreateTableOperator(tableName, columns);

            _mockCatalog.Setup(c => c.CreateTable(tableName, columns))
                        .Throws(new InvalidOperationException("Table already exists"));

            // Act
            Action act = () => op.Execute(_context);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("Table already exists");
        }
    }
}