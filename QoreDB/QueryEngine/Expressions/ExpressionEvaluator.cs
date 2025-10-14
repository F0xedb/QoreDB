using System;
using System.Collections.Generic;

namespace QoreDB.QueryEngine.Expressions
{
    /// <summary>
    /// Evaluates an expression tree against a given row of data
    /// </summary>
    public class ExpressionEvaluator
    {
        
        /// <summary>
        /// Evaluates the given expression and returns the result
        /// </summary>
        /// <param name="expression">The expression tree to evaluate</param>
        /// <param name="row">The row of data to evaluate the expression against</param>
        /// <returns>The result of the evaluation, typically a <see cref="bool"/> or a number</returns>
        public object Evaluate(IExpression expression, IDictionary<string, object> row)
        {
            return expression switch
            {
                LiteralValue lv => lv.Value,
                ColumnValue cv => row[cv.ColumnName],
                BinaryExpression be => EvaluateBinary(be, row),
                LogicalExpression le => EvaluateLogical(le, row),
                _ => throw new NotSupportedException($"Unsupported expression type: {expression}")
            };
        }

        /// <summary>
        /// Handles the evaluation of a binary expression, which includes both arithmetic and comparison operations
        /// </summary>
        /// <param name="be">The binary expression to evaluate</param>
        /// <param name="row">The row of data used for the evaluation</param>
        /// <returns>The result of the binary operation</returns>
        private object EvaluateBinary(BinaryExpression be, IDictionary<string, object> row)
        {
            var left = Evaluate(be.Left, row);
            var right = Evaluate(be.Right, row);

            // A real engine would have robust type checking and casting
            if (IsComparisonOperator(be.Operator))
            {
                var convertedRight = Convert.ChangeType(right, left.GetType());
                return be.Operator switch
                {
                    OperatorType.Equal => ((IComparable)left).CompareTo(convertedRight) == 0,
                    OperatorType.GreaterThan => ((IComparable)left).CompareTo(convertedRight) > 0,
                    OperatorType.LessThan => ((IComparable)left).CompareTo(convertedRight) < 0,
                    OperatorType.GreaterThanOrEqual => ((IComparable)left).CompareTo(convertedRight) >= 0,
                    OperatorType.LessThanOrEqual => ((IComparable)left).CompareTo(convertedRight) <= 0,
                    _ => throw new NotSupportedException($"Unsupported comparison operator: {be.Operator}")
                };
            }
            
            var leftNum = Convert.ToDouble(left);
            var rightNum = Convert.ToDouble(right);

            if (be.Operator == OperatorType.Divide && rightNum == 0)
            {
                throw new DivideByZeroException();
            }

            return be.Operator switch
            {
                OperatorType.Add => leftNum + rightNum,
                OperatorType.Subtract => leftNum - rightNum,
                OperatorType.Multiply => leftNum * rightNum,
                OperatorType.Divide => leftNum / rightNum,
                _ => throw new NotSupportedException($"Unsupported arithmetic operator: {be.Operator}")
            };
        }

        /// <summary>
        /// Handles the evaluation of a logical expression, such as <see langword="AND"/> or <see langword="OR"/>
        /// </summary>
        /// <param name="le">The logical expression to evaluate</param>
        /// <param name="row">The row of data used for the evaluation</param>
        /// <returns><see langword="true"/> if the logical expression is satisfied, otherwise <see langword="false"/></returns>
        private bool EvaluateLogical(LogicalExpression le, IDictionary<string, object> row)
        {
            var left = (bool)Evaluate(le.Left, row);

            // Short-circuit evaluation
            if (le.Operator == OperatorType.And && !left) return false;
            if (le.Operator == OperatorType.Or && left) return true;
            
            var right = (bool)Evaluate(le.Right, row);

            return le.Operator switch
            {
                OperatorType.And => left && right,
                OperatorType.Or => left || right,
                _ => throw new NotSupportedException($"Unsupported logical operator: {le.Operator}")
            };
        }

        private bool IsComparisonOperator(OperatorType op)
        {
            return op == OperatorType.Equal ||
                   op == OperatorType.GreaterThan ||
                   op == OperatorType.LessThan ||
                   op == OperatorType.GreaterThanOrEqual ||
                   op == OperatorType.LessThanOrEqual;
        }
    }
}