using NUnit.Framework;
using FluentAssertions;
using QoreDB.QueryEngine.Expressions;
using QoreDB.QueryEngine.Optimizer.Visitors;

namespace QoreDB.QueryEngine.Tests.Optimizer
{
    [TestFixture]
    public class ConstantFoldingExpressionVisitorTests
    {
        private ConstantFoldingExpressionVisitor _visitor;

        [SetUp]
        public void Setup()
        {
            _visitor = new ConstantFoldingExpressionVisitor();
        }

        [Test]
        public void Visit_SimpleBinaryExpression_FoldsToLiteral()
        {
            // Arrange
            var expression = new BinaryExpression(
                new LiteralValue(10),
                OperatorType.Add,
                new LiteralValue(20)
            );

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            result.Should().BeOfType<LiteralValue>()
                .Which.Value.Should().Be(30.0);
        }

        [Test]
        public void Visit_NestedBinaryExpression_FoldsToLiteral()
        {
            // Arrange
            var expression = new BinaryExpression(
                new LiteralValue(10),
                OperatorType.Add,
                new BinaryExpression(
                    new LiteralValue(5),
                    OperatorType.Multiply,
                    new LiteralValue(2)
                )
            );

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            result.Should().BeOfType<LiteralValue>()
                .Which.Value.Should().Be(20.0);
        }

        [Test]
        public void Visit_LogicalExpressionWithFoldableSide_FoldsCorrectly()
        {
            // Arrange
            var expression = new LogicalExpression(
                new ColumnValue("Age"),
                OperatorType.Or,
                new BinaryExpression(
                    new LiteralValue(10),
                    OperatorType.GreaterThan,
                    new LiteralValue(5)
                )
            );

            // Act
            var result = _visitor.Visit(expression) as LogicalExpression;

            // Assert
            result.Should().NotBeNull();
            result.Left.Should().BeOfType<ColumnValue>();
            result.Right.Should().BeOfType<LiteralValue>()
                .Which.Value.Should().Be(true);
        }

        [Test]
        public void Visit_AndExpressionWithTwoFoldableSides_FoldsToSingleLiteral()
        {
            // Arrange
            var expression = new LogicalExpression(
                new BinaryExpression(
                    new LiteralValue(10),
                    OperatorType.GreaterThan,
                    new LiteralValue(5)
                ),
                OperatorType.And,
                new BinaryExpression(
                    new LiteralValue(20),
                    OperatorType.LessThan,
                    new LiteralValue(30)
                )
            );

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            result.Should().BeOfType<LiteralValue>()
                .Which.Value.Should().Be(true);
        }

        [Test]
        public void Visit_ExpressionWithColumn_DoesNotFold()
        {
            // Arrange
            var expression = new BinaryExpression(
                new ColumnValue("Price"),
                OperatorType.Multiply,
                new LiteralValue(1.2)
            );

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            result.Should().BeOfType<BinaryExpression>();
        }
    }
}