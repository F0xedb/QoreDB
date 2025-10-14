using FluentAssertions;
using Moq;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.QueryEngine.Interfaces;
using ExecutionContext = QoreDB.QueryEngine.Execution.ExecutionContext;

namespace Qore.UnitTests.QueryEngine.Execution.Operators
{
    [TestFixture]
    public class ProjectionOperatorTests
    {
        private Mock<IExecutionOperator> _mockSource;
        private ExecutionContext _context;

        [SetUp]
        public void SetUp()
        {
            _mockSource = new Mock<IExecutionOperator>();
            _context = new ExecutionContext(null); // No catalog needed for this test
        }

        [Test]
        public void Execute_WhenSourceHasData_ReturnsProjectedRows()
        {
            // Arrange
            var sourceRows = new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "Name", "Alice" }, { "Age", 30 } },
                new() { { "Id", 2 }, { "Name", "Bob" }, { "Age", 25 } }
            };
            _mockSource.Setup(s => s.Execute(It.IsAny<IExecutionContext>()))
                       .Returns(new RowsQueryResult(sourceRows));

            var op = new ProjectionOperator(_mockSource.Object, new List<string> { "Id", "Name" });

            // Act
            var result = op.Execute(_context) as RowsQueryResult;
            var rows = result.Rows.ToList();

            // Assert
            result.Should().NotBeNull();
            rows.Should().HaveCount(2);
            rows[0].Should().ContainKeys("Id", "Name");
            rows[0].Should().NotContainKey("Age");
            rows[1]["Name"].Should().Be("Bob");
        }
    }
}