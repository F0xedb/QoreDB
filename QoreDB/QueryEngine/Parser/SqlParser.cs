using Antlr4.Runtime;
using QoreDB.QueryEngine.Execution;
using QoreDB.QueryEngine.Interfaces;
using QoreDB.QueryEngine.Parser.Antlr; // Use the new namespace
using System;

namespace QoreDB.QueryEngine.Parser
{
    /// <summary>
    /// A parser that uses an ANTLR-generated parser and a visitor to build the execution plan
    /// </summary>
    public class SQLParser : ISqlParser
    {
        public QueryExecutionPlan Parse(string query)
        {
            var inputStream = new AntlrInputStream(query);
            var lexer = new SqlLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new SqlParser(commonTokenStream);

            // Add a custom error listener to throw exceptions on syntax errors
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ThrowingErrorListener());

            // Start parsing from the 'root' rule in the grammar
            var tree = parser.root();

            var astBuilder = new AstBuilder();
            var rootOperator = astBuilder.Visit(tree);

            return new QueryExecutionPlan(rootOperator);
        }
    }
    
    /// <summary>
    /// A custom ANTLR error listener that throws a C# exception on syntax errors
    /// </summary>
    public class ThrowingErrorListener : BaseErrorListener
    {
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new ArgumentException($"Invalid SQL syntax: {msg}", e);
        }
    }
}