using FluentAssertions;
using Moq;
using QoreDB.Catalog.Interfaces;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Execution.Operators;
using ExecutionContext = QoreDB.QueryEngine.Execution.ExecutionContext;

namespace Qore.UnitTests.QueryEngine.Execution.Operators
{
    [TestFixture]
    public class DropTableOperatorTests
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
        public void Execute_WhenTableExists_CallsCatalogDropTable()
        {
            // Arrange
            var tableName = "UsersToDelete";
            var op = new DropTableOperator(tableName, false);

            // Act
            var result = op.Execute(_context) as MessageQueryResult;

            // Assert
            _mockCatalog.Verify(c => c.DropTable(tableName, false), Times.Once);
            result.Should().NotBeNull();
            result.Message.Should().Be($"Table '{tableName}' dropped successfully");
        }

        [Test]
        public void Execute_WhenCatalogThrows_ExceptionPropagates()
        {
            // Arrange
            var tableName = "NonExistentTable";
            var op = new DropTableOperator(tableName, false);

            _mockCatalog.Setup(c => c.DropTable(tableName, false))
                        .Throws(new InvalidOperationException("Table not found"));

            // Act
            Action act = () => op.Execute(_context);

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("Table not found");
        }
    }
}