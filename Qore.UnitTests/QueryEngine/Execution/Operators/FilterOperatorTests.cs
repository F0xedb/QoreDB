using FluentAssertions;
using Moq;
using NUnit.Framework;
using QoreDB.QueryEngine.Execution;
using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.QueryEngine.Expressions;
using QoreDB.QueryEngine.Interfaces;
using System.Collections.Generic;

namespace Qore.UnitTests.QueryEngine.Execution.Operators
{
    [TestFixture]
    public class FilterOperatorTests
    {
        [Test]
        public void Execute_WhenPredicateMatches_ReturnsFilteredRows()
        {
            // Arrange
            var sourceRows = new List<Dictionary<string, object>>
            {
                new() { { "Id", 1 }, { "City", "Seattle" } },
                new() { { "Id", 2 }, { "City", "New York" } },
                new() { { "Id", 3 }, { "City", "Seattle" } }
            };
            var mockSource = new Mock<IExecutionOperator>();
            mockSource.Setup(s => s.Execute(It.IsAny<IExecutionContext>()))
                      .Returns(new RowsQueryResult(sourceRows));

            var predicate = new BinaryExpression(
                new ColumnValue("City"),
                OperatorType.Equal,
                new LiteralValue("Seattle")
            );

            var op = new FilterOperator(mockSource.Object, predicate);

            // Act
            var result = op.Execute(null) as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
            result.Rows.Should().OnlyContain(r => (string)r["City"] == "Seattle");
        }
    }
}