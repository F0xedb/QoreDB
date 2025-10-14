using FluentAssertions;
using Moq;
using NUnit.Framework;
using QoreDB.QueryEngine.Execution;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.QueryEngine.Interfaces;
using System.Collections.Generic;

namespace Qore.UnitTests.QueryEngine.Execution.Operators
{
    [TestFixture]
    public class SortOperatorTests
    {
        [Test]
        public void Execute_WhenCalled_ReturnsSortedRows()
        {
            // Arrange
            var sourceRows = new List<Dictionary<string, object>>
            {
                new() { { "Name", "Charlie" } },
                new() { { "Name", "Alice" } },
                new() { { "Name", "Bob" } }
            };
            var mockSource = new Mock<IExecutionOperator>();
            mockSource.Setup(s => s.Execute(It.IsAny<IExecutionContext>()))
                      .Returns(new RowsQueryResult(sourceRows));
            
            var op = new SortOperator(mockSource.Object, "Name", isAscending: true);

            // Act
            var result = op.Execute(null) as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(3);
            result.Rows.Should().BeInAscendingOrder(r => r["Name"]);
        }
    }
}