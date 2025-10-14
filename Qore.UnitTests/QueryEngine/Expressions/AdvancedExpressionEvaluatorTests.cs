using NUnit.Framework;
using QoreDB.QueryEngine.Expressions;
using System.Collections.Generic;

namespace QoreDB.UnitTests.QueryEngine.Expressions
{
    [TestFixture]
    public class AdvancedExpressionEvaluatorTests
    {
        private ExpressionEvaluator _evaluator;
        private Dictionary<string, object> _row;

        [SetUp]
        public void Setup()
        {
            _evaluator = new ExpressionEvaluator();
            _row = new Dictionary<string, object>
            {
                { "ID", 10 },
                { "Age", 25 },
                { "Name", "John" },
                { "Salary", 50000.0 }
            };
        }

        #region Arithmetic Expression Tests

        [Test]
        public void Evaluate_AdditionExpression_ReturnsCorrectSum()
        {
            // Expression: 10 + 5
            var expression = new BinaryExpression(new LiteralValue(10), OperatorType.Add, new LiteralValue(5));
            var result = _evaluator.Evaluate(expression, _row);
            Assert.AreEqual(15.0, result);
        }

        [Test]
        public void Evaluate_SubtractionExpression_ReturnsCorrectDifference()
        {
            // Expression: Age - 5
            var expression = new BinaryExpression(new ColumnValue("Age"), OperatorType.Subtract, new LiteralValue(5));
            var result = _evaluator.Evaluate(expression, _row);
            Assert.AreEqual(20.0, result);
        }

        [Test]
        public void Evaluate_MultiplicationExpression_ReturnsCorrectProduct()
        {
            // Expression: ID * 2
            var expression = new BinaryExpression(new ColumnValue("ID"), OperatorType.Multiply, new LiteralValue(2));
            var result = _evaluator.Evaluate(expression, _row);
            Assert.AreEqual(20.0, result);
        }

        [Test]
        public void Evaluate_DivisionExpression_ReturnsCorrectQuotient()
        {
            // Expression: Salary / 2
            var expression = new BinaryExpression(new ColumnValue("Salary"), OperatorType.Divide, new LiteralValue(2));
            var result = _evaluator.Evaluate(expression, _row);
            Assert.AreEqual(25000.0, result);
        }

        [Test]
        public void Evaluate_DivisionByZero_ThrowsException()
        {
            // Expression: ID / 0
            var expression = new BinaryExpression(new ColumnValue("ID"), OperatorType.Divide, new LiteralValue(0));
            Assert.Throws<System.DivideByZeroException>(() => _evaluator.Evaluate(expression, _row));
        }

        #endregion

        #region Logical Expression Tests

        [Test]
        public void Evaluate_AndExpression_TrueAndTrue_ReturnsTrue()
        {
            // Expression: ID = 10 AND Age > 20
            var left = new BinaryExpression(new ColumnValue("ID"), OperatorType.Equal, new LiteralValue(10));
            var right = new BinaryExpression(new ColumnValue("Age"), OperatorType.GreaterThan, new LiteralValue(20));
            var expression = new LogicalExpression(left, OperatorType.And, right);
            var result = _evaluator.Evaluate(expression, _row);
            Assert.IsTrue((bool)result);
        }

        [Test]
        public void Evaluate_AndExpression_TrueAndFalse_ReturnsFalse()
        {
            // Expression: ID = 10 AND Age < 20
            var left = new BinaryExpression(new ColumnValue("ID"), OperatorType.Equal, new LiteralValue(10));
            var right = new BinaryExpression(new ColumnValue("Age"), OperatorType.LessThan, new LiteralValue(20));
            var expression = new LogicalExpression(left, OperatorType.And, right);
            var result = _evaluator.Evaluate(expression, _row);
            Assert.IsFalse((bool)result);
        }
        
        [Test]
        public void Evaluate_OrExpression_TrueOrFalse_ReturnsTrue()
        {
            // Expression: ID = 10 OR Age < 20
            var left = new BinaryExpression(new ColumnValue("ID"), OperatorType.Equal, new LiteralValue(10));
            var right = new BinaryExpression(new ColumnValue("Age"), OperatorType.LessThan, new LiteralValue(20));
            var expression = new LogicalExpression(left, OperatorType.Or, right);
            var result = _evaluator.Evaluate(expression, _row);
            Assert.IsTrue((bool)result);
        }

        [Test]
        public void Evaluate_OrExpression_FalseOrFalse_ReturnsFalse()
        {
            // Expression: ID = 5 OR Age < 20
            var left = new BinaryExpression(new ColumnValue("ID"), OperatorType.Equal, new LiteralValue(5));
            var right = new BinaryExpression(new ColumnValue("Age"), OperatorType.LessThan, new LiteralValue(20));
            var expression = new LogicalExpression(left, OperatorType.Or, right);
            var result = _evaluator.Evaluate(expression, _row);
            Assert.IsFalse((bool)result);
        }

        #endregion

        #region Operator Precedence Tests

        [Test]
        public void Evaluate_Precedence_AndBeforeOr()
        {
            // Expression: false OR true AND true (evaluates to false OR true => true)
            var exp1 = new LiteralValue(false);
            var exp2 = new LiteralValue(true);
            var exp3 = new LiteralValue(true);
            var andExp = new LogicalExpression(exp2, OperatorType.And, exp3);
            var orExp = new LogicalExpression(exp1, OperatorType.Or, andExp);
            
            var result = _evaluator.Evaluate(orExp, _row);
            Assert.IsTrue((bool)result);
        }

        [Test]
        public void Evaluate_Precedence_MultiplicationBeforeAddition()
        {
            // Expression: 2 + 3 * 4 (evaluates to 2 + 12 => 14)
            var exp1 = new LiteralValue(2);
            var exp2 = new LiteralValue(3);
            var exp3 = new LiteralValue(4);
            var mulExp = new BinaryExpression(exp2, OperatorType.Multiply, exp3);
            var addExp = new BinaryExpression(exp1, OperatorType.Add, mulExp);

            var result = _evaluator.Evaluate(addExp, _row);
            Assert.AreEqual(14.0, result);
        }

        [Test]
        public void Evaluate_ParenthesizedExpression_OverridesPrecedence()
        {
            // Expression: (2 + 3) * 4 (evaluates to 5 * 4 => 20)
            var exp1 = new LiteralValue(2);
            var exp2 = new LiteralValue(3);
            var exp3 = new LiteralValue(4);
            var addExp = new BinaryExpression(exp1, OperatorType.Add, exp2);
            var mulExp = new BinaryExpression(addExp, OperatorType.Multiply, exp3);

            var result = _evaluator.Evaluate(mulExp, _row);
            Assert.AreEqual(20.0, result);
        }

        #endregion

        #region Comparison Operator Tests

        [Test]
        public void Evaluate_GreaterThanOrEqual_ReturnsCorrectResult()
        {
            // Expression: Age >= 25
            var expression = new BinaryExpression(new ColumnValue("Age"), OperatorType.GreaterThanOrEqual, new LiteralValue(25));
            var result = _evaluator.Evaluate(expression, _row);
            Assert.IsTrue((bool)result);
        }

        [Test]
        public void Evaluate_LessThanOrEqual_ReturnsCorrectResult()
        {
            // Expression: ID <= 10
            var expression = new BinaryExpression(new ColumnValue("ID"), OperatorType.LessThanOrEqual, new LiteralValue(10));
            var result = _evaluator.Evaluate(expression, _row);
            Assert.IsTrue((bool)result);
        }

        #endregion
    }
}