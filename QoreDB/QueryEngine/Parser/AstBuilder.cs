using Antlr4.Runtime.Misc;
using QoreDB.Catalog.Models;
using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.QueryEngine.Interfaces;
using QoreDB.QueryEngine.Parser.Antlr;
using QoreDB.QueryEngine.Expressions;

namespace QoreDB.QueryEngine.Parser
{
    /// <summary>
    /// This visitor walks the ANTLR parse tree and builds our operator-based query plan (AST)
    /// </summary>
    public class AstBuilder : SqlBaseVisitor<IExecutionOperator>
    {
        private readonly ExpressionVisitor _expressionVisitor = new ExpressionVisitor();

        public override IExecutionOperator VisitInsert_statement([NotNull] SqlParser.Insert_statementContext context)
        {
            var tableName = context.table_name.Text;
            var columns = context.column_list().ID().Select(c => c.GetText()).ToList();
            var values = context.value_list().value().Select(v => ParseValue(v)).ToList();

            if (columns.Count != values.Count)
            {
                throw new InvalidOperationException("Column count does not match value count");
            }

            var row = new Dictionary<string, object>();
            for (var i = 0; i < columns.Count; i++)
            {
                row[columns[i]] = values[i];
            }

            return new InsertOperator(tableName, row);
        }

        public override IExecutionOperator VisitSelect_statement([NotNull] SqlParser.Select_statementContext context)
        {
            // The table source is now a separate rule in the grammar
            var tableName = context.table_source().table_name.Text;
            
            // For SELECT *, we will let the ProjectionOperator handle it by passing null.
            List<string> columns = null;

            // Build the operator tree from the bottom up.
            IExecutionOperator plan = new TableScanOperator(tableName);

            // Add a FilterOperator if a WHERE clause exists.
            if (context.where_clause() is { } where)
            {
                // Use the dedicated expression visitor
                var predicate = _expressionVisitor.Visit(where.expression());
                plan = new FilterOperator(plan, predicate);
            }

            // Add a SortOperator if an ORDER BY clause exists.
            if (context.order_by_clause() is { } orderBy)
            {
                var sortColumn = orderBy.column_name.Text;
                var isAsc = orderBy.direction == null || orderBy.direction.Type == SqlLexer.ASC;
                plan = new SortOperator(plan, sortColumn, isAsc);
            }

            // Add a TakeOperator if a LIMIT clause exists
            if (context.limit_clause() is { } limit)
            {
                var amount = int.Parse(limit.amount.Text);
                var offset = limit.offset == null ? 0 : int.Parse(limit.offset.Text);
                plan = new TakeOperator(plan, amount, offset);
            }

            if (context.column_list().ASTERISK() == null)
            {
                columns = context.column_list().ID().Select(c => c.GetText()).ToList();
            }

            // The ProjectionOperator is always at the top for SELECT.
            plan = new ProjectionOperator(plan, columns);

            return plan;
        }

        public override IExecutionOperator VisitCreate_table_statement([NotNull] SqlParser.Create_table_statementContext context)
        {
            var tableName = context.table_name.Text;
            var columns = context.column_definitions().column_def().Select(cd =>
            {
                var columnName = cd.column_name.Text;
                var typeName = cd.data_type.Text.ToUpperInvariant();
                Type columnType = typeName switch
                {
                    "INT" => typeof(int),
                    "STRING" => typeof(string),
                    _ => throw new NotSupportedException($"Unsupported data type: {typeName}")
                };
                return new ColumnInfo(columnName, columnType);
            }).ToList();

            return new CreateTableOperator(tableName, columns);
        }

        public override IExecutionOperator VisitDrop_table_statement([NotNull] SqlParser.Drop_table_statementContext context)
        {
            var tableName = context.table_name.Text;
            var ifExists = context.IF() != null;
            return new DropTableOperator(tableName, ifExists);
        }

        private object ParseValue(SqlParser.ValueContext context)
        {
            if (context.STRING_LITERAL() is { } s)
            {
                // Remove the surrounding single quotes
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