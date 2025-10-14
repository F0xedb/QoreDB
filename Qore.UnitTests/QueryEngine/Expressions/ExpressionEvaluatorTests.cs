using FluentAssertions;
using NUnit.Framework;
using QoreDB.QueryEngine.Expressions;
using System.Collections.Generic;

namespace Qore.UnitTests.QueryEngine.Expressions
{
    [TestFixture]
    public class ExpressionEvaluatorTests
    {
        private ExpressionEvaluator _evaluator;
        private Dictionary<string, object> _sampleRow;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new ExpressionEvaluator();
            _sampleRow = new Dictionary<string, object>
            {
                { "Id", 10 },
                { "Name", "QoreDB" },
                { "Price", 100 }
            };
        }

        [Test]
        public void Evaluate_EqualExpression_WhenValuesAreEqual_ReturnsTrue()
        {
            // Arrange
            var expression = new BinaryExpression(
                new ColumnValue("Id"),
                OperatorType.Equal,
                new LiteralValue(10)
            );

            // Act
            var result = _evaluator.Evaluate(expression, _sampleRow);

            // Assert
            result.Should().Be(true);
        }

        [Test]
        public void Evaluate_EqualExpression_WhenValuesAreNotEqual_ReturnsFalse()
        {
            // Arrange
            var expression = new BinaryExpression(
                new ColumnValue("Name"),
                OperatorType.Equal,
                new LiteralValue("OtherDB")
            );

            // Act
            var result = _evaluator.Evaluate(expression, _sampleRow);

            // Assert
            result.Should().Be(false);
        }

        [Test]
        public void Evaluate_GreaterThanExpression_WhenLeftIsGreater_ReturnsTrue()
        {
            // Arrange
            var expression = new BinaryExpression(
                new ColumnValue("Price"),
                OperatorType.GreaterThan,
                new LiteralValue(50)
            );

            // Act
            var result = _evaluator.Evaluate(expression, _sampleRow);

            // Assert
            result.Should().Be(true);
        }

        [Test]
        public void Evaluate_LessThanExpression_WhenLeftIsLess_ReturnsTrue()
        {
            // Arrange
            var expression = new BinaryExpression(
                new ColumnValue("Price"),
                OperatorType.LessThan,
                new LiteralValue(200)
            );

            // Act
            var result = _evaluator.Evaluate(expression, _sampleRow);

            // Assert
            result.Should().Be(true);
        }
        
        [Test]
        public void Evaluate_LessThanExpression_WhenValuesAreEqual_ReturnsFalse()
        {
            // Arrange
            var expression = new BinaryExpression(
                new ColumnValue("Price"),
                OperatorType.LessThan,
                new LiteralValue(100)
            );

            // Act
            var result = _evaluator.Evaluate(expression, _sampleRow);

            // Assert
            result.Should().Be(false);
        }
    }
}