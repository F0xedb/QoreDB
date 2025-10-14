using Antlr4.Runtime.Misc;
using QoreDB.QueryEngine.Expressions;
using QoreDB.QueryEngine.Parser.Antlr;
using System;

namespace QoreDB.QueryEngine.Parser
{
    /// <summary>
    /// Visits the expression nodes of a parse tree and builds an <see cref="IExpression"/> tree
    /// </summary>
    public class ExpressionVisitor : SqlBaseVisitor<IExpression>
    {
        /// <summary>
        /// Visits a logical OR expression and constructs a <see cref="LogicalExpression"/>
        /// </summary>
        /// <param name="context">The parse tree context for the logical OR expression</param>
        /// <returns>A new <see cref="LogicalExpression"/> representing the OR operation</returns>
        public override IExpression VisitLogicalOr([NotNull] SqlParser.LogicalOrContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);
            return new LogicalExpression(left, OperatorType.Or, right);
        }

        /// <summary>
        /// Visits a logical AND expression and constructs a <see cref="LogicalExpression"/>
        /// </summary>
        /// <param name="context">The parse tree context for the logical AND expression</param>
        /// <returns>A new <see cref="LogicalExpression"/> representing the AND operation</returns>
        public override IExpression VisitLogicalAnd([NotNull] SqlParser.LogicalAndContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);
            return new LogicalExpression(left, OperatorType.And, right);
        }

        /// <summary>
        /// Visits a comparison expression and constructs a <see cref="BinaryExpression"/>
        /// </summary>
        /// <param name="context">The parse tree context for the comparison expression</param>
        /// <returns>A new <see cref="BinaryExpression"/> representing the comparison</returns>
        public override IExpression VisitComparison([NotNull] SqlParser.ComparisonContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);
            var op = context.op.Type switch
            {
                SqlLexer.EQUAL => OperatorType.Equal,
                SqlLexer.LESS_THAN => OperatorType.LessThan,
                SqlLexer.GREATER_THAN => OperatorType.GreaterThan,
                SqlLexer.GTE => OperatorType.GreaterThanOrEqual,
                SqlLexer.LTE => OperatorType.LessThanOrEqual,
                _ => throw new NotSupportedException("Unsupported comparison operator")
            };

            return new BinaryExpression(left, op, right);
        }

        /// <summary>
        /// Visits an additive expression and constructs a <see cref="BinaryExpression"/>
        /// </summary>
        /// <param name="context">The parse tree context for the additive expression</param>
        /// <returns>A new <see cref="BinaryExpression"/> representing the addition or subtraction</returns>
        public override IExpression VisitAdd([NotNull] SqlParser.AddContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);
            var op = context.op.Type switch
            {
                SqlLexer.PLUS => OperatorType.Add,
                SqlLexer.MINUS => OperatorType.Subtract,
                _ => throw new NotSupportedException("Unsupported additive operator")
            };

            return new BinaryExpression(left, op, right);
        }

        /// <summary>
        /// Visits a multiplicative expression and constructs a <see cref="BinaryExpression"/>
        /// </summary>
        /// <param name="context">The parse tree context for the multiplicative expression</param>
        /// <returns>A new <see cref="BinaryExpression"/> representing the multiplication or division</returns>
        public override IExpression VisitMul([NotNull] SqlParser.MulContext context)
        {
            var left = Visit(context.left);
            var right = Visit(context.right);
            var op = context.op.Type switch
            {
                SqlLexer.ASTERISK => OperatorType.Multiply,
                SqlLexer.SLASH => OperatorType.Divide,
                _ => throw new NotSupportedException("Unsupported multiplicative operator")
            };

            return new BinaryExpression(left, op, right);
        }

        /// <summary>
        /// Visits a parenthesized expression, effectively unwrapping it
        /// </summary>
        /// <param name="context">The parse tree context for the parenthesized expression</param>
        /// <returns>The inner expression, with precedence determined by the parentheses</returns>
        public override IExpression VisitParenthesizedExpression([NotNull] SqlParser.ParenthesizedExpressionContext context)
        {
            return Visit(context.expression());
        }

        /// <summary>
        /// Visits a literal value and constructs a <see cref="LiteralValue"/>
        /// </summary>
        /// <param name="context">The parse tree context for the literal value</param>
        /// <returns>A new <see cref="LiteralValue"/> representing the parsed literal</returns>
        public override IExpression VisitLiteral([NotNull] SqlParser.LiteralContext context)
        {
            return new LiteralValue(ParseValue(context.value()));
        }

        /// <summary>
        /// Visits a column identifier and constructs a <see cref="ColumnValue"/>
        /// </summary>
        /// <param name="context">The parse tree context for the column identifier</param>
        /// <returns>A new <see cref="ColumnValue"/> representing the column reference</returns>
        public override IExpression VisitColumn([NotNull] SqlParser.ColumnContext context)
        {
            return new ColumnValue(context.ID().GetText());
        }

        /// <summary>
        /// Delegates the visit to the nested boolean expression
        /// </summary>
        /// <param name="context">The parse tree context for the nested boolean expression</param>
        /// <returns>The result of visiting the nested boolean expression</returns>
        public override IExpression VisitNestedBoolean([NotNull] SqlParser.NestedBooleanContext context)
            => Visit(context.boolean_expression());

        /// <summary>
        /// Delegates the visit to the nested comparison expression
        /// </summary>
        /// <param name="context">The parse tree context for the nested comparison expression</param>
        /// <returns>The result of visiting the nested comparison expression</returns>
        public override IExpression VisitNestedComparison([NotNull] SqlParser.NestedComparisonContext context)
            => Visit(context.comparison_expression());
        
        /// <summary>
        /// Delegates the visit to the nested additive expression
        /// </summary>
        /// <param name="context">The parse tree context for the nested additive expression</param>
        /// <returns>The result of visiting the nested additive expression</returns>
        public override IExpression VisitNestedAdditive([NotNull] SqlParser.NestedAdditiveContext context)
            => Visit(context.additive_expression());

        /// <summary>
        /// Delegates the visit to the nested multiplicative expression
        /// </summary>
        /// <param name="context">The parse tree context for the nested multiplicative expression</param>
        /// <returns>The result of visiting the nested multiplicative expression</returns>
        public override IExpression VisitNestedMultiplicative([NotNull] SqlParser.NestedMultiplicativeContext context)
            => Visit(context.multiplicative_expression());
        
        /// <summary>
        /// Delegates the visit to the nested primary expression
        /// </summary>
        /// <param name="context">The parse tree context for the nested primary expression</param>
        /// <returns>The result of visiting the nested primary expression</returns>
        public override IExpression VisitNestedPrimary([NotNull] SqlParser.NestedPrimaryContext context)
            => Visit(context.primary_expression());

        /// <summary>
        /// Parses a value context from the parse tree into a C# object
        /// </summary>
        /// <param name="context">The parse tree context for the value</param>
        /// <returns>The parsed value as either a <see cref="string"/> or an <see cref="int"/></returns>
        private object ParseValue(SqlParser.ValueContext context)
        {
            if (context.STRING_LITERAL() is { } s)
            {
                return s.GetText().Trim('\'');
            }
            if (context.NUMBER() is { } n)
            {
                return int.Parse(n.GetText());
            }
            return null;
        }
    }
}