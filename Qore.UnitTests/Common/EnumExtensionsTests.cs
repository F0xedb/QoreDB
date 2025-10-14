using NUnit.Framework;
using QoreDB.QueryEngine.Expressions;
using FluentAssertions;
using QoreDB.Common.Attributes;
using QoreDB.Common.Extensions;

namespace QoreDB.UnitTests.Common
{
    [TestFixture]
    public class EnumExtensionsTests
    {
        // A sample enum for testing purposes that uses the attribute
        private enum TestEnumWithAttribute
        {
            [EnumString("Value A")]
            A,
            [EnumString("Value B")]
            B
        }
        
        // A sample enum without the attribute to test fallback behavior
        private enum TestEnumWithoutAttribute
        {
            C,
            D
        }

        [Test]
        public void GetString_WhenEnumHasAttribute_ReturnsAttributeValue()
        {
            // Arrange
            var enumValue = TestEnumWithAttribute.A;

            // Act
            var result = enumValue.GetString();

            // Assert
            result.Should().Be("Value A");
        }
        
        [Test]
        public void GetString_WhenEnumDoesNotHaveAttribute_ReturnsEnumName()
        {
            // Arrange
            var enumValue = TestEnumWithoutAttribute.C;

            // Act
            var result = enumValue.GetString();

            // Assert
            result.Should().Be("C");
        }

        [Test]
        public void ToString_ForBinaryExpression_UsesGetString()
        {
            // Arrange
            var expression = new BinaryExpression(
                new LiteralValue(1),
                OperatorType.GreaterThan,
                new LiteralValue(2)
            );

            // Act
            var result = expression.ToString();

            // Assert
            result.Should().Be("(1 > 2)");
        }
        
        [Test]
        public void ToString_ForLogicalExpression_UsesGetString()
        {
            // Arrange
            var expression = new LogicalExpression(
                new LiteralValue(true),
                OperatorType.And,
                new LiteralValue(false)
            );

            // Act
            var result = expression.ToString();

            // Assert
            result.Should().Be("(True AND False)");
        }
    }
}

