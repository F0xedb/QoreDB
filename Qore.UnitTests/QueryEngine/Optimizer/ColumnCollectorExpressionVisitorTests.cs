using NUnit.Framework;
using FluentAssertions;
using QoreDB.QueryEngine.Expressions;
using QoreDB.QueryEngine.Optimizer.Visitors;

namespace QoreDB.QueryEngine.Tests.Optimizer
{
    [TestFixture]
    public class ColumnCollectorExpressionVisitorTests
    {
        private ColumnCollectorExpressionVisitor _visitor;

        [SetUp]
        public void Setup()
        {
            _visitor = new ColumnCollectorExpressionVisitor();
        }

        [Test]
        public void Visit_BinaryExpressionWithOneColumn_CollectsOneColumn()
        {
            // Arrange
            var expression = new BinaryExpression(
                new ColumnValue("Price"),
                OperatorType.GreaterThan,
                new LiteralValue(100)
            );

            // Act
            _visitor.Visit(expression);

            // Assert
            _visitor.Columns.Should().BeEquivalentTo("Price");
        }

        [Test]
        public void Visit_BinaryExpressionWithTwoColumns_CollectsBothColumns()
        {
            // Arrange
            var expression = new BinaryExpression(
                new ColumnValue("Price"),
                OperatorType.LessThan,
                new ColumnValue("Cost")
            );

            // Act
            _visitor.Visit(expression);

            // Assert
            _visitor.Columns.Should().BeEquivalentTo("Price", "Cost");
        }

        [Test]
        public void Visit_LogicalExpression_CollectsColumnsFromBothSides()
        {
            // Arrange
            var expression = new LogicalExpression(
                new BinaryExpression(new ColumnValue("Price"), OperatorType.GreaterThan, new LiteralValue(50)),
                OperatorType.And,
                new BinaryExpression(new ColumnValue("Quantity"), OperatorType.LessThan, new LiteralValue(10))
            );

            // Act
            _visitor.Visit(expression);

            // Assert
            _visitor.Columns.Should().BeEquivalentTo("Price", "Quantity");
        }

        [Test]
        public void Visit_NestedExpression_CollectsAllColumns()
        {
            // Arrange
            var expression = new BinaryExpression(
                new ColumnValue("Total"),
                OperatorType.Equal,
                new BinaryExpression(
                    new ColumnValue("Price"),
                    OperatorType.Multiply,
                    new ColumnValue("Quantity")
                )
            );

            // Act
            _visitor.Visit(expression);

            // Assert
            _visitor.Columns.Should().BeEquivalentTo("Total", "Price", "Quantity");
        }

        [Test]
        public void Visit_ExpressionWithDuplicateColumns_CollectsUniqueColumns()
        {
            // Arrange
            var expression = new LogicalExpression(
                new BinaryExpression(new ColumnValue("Price"), OperatorType.GreaterThan, new LiteralValue(10)),
                OperatorType.Or,
                new BinaryExpression(new ColumnValue("Price"), OperatorType.LessThan, new LiteralValue(5))
            );

            // Act
            _visitor.Visit(expression);

            // Assert
            _visitor.Columns.Should().HaveCount(1);
            _visitor.Columns.Should().Contain("Price");
        }

        [Test]
        public void Visit_ExpressionWithNoColumns_CollectsNothing()
        {
            // Arrange
            var expression = new BinaryExpression(
                new LiteralValue(10),
                OperatorType.Add,
                new LiteralValue(20)
            );

            // Act
            _visitor.Visit(expression);

            // Assert
            _visitor.Columns.Should().BeEmpty();
        }
    }
}
