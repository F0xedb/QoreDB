using QoreDB.Tui.Tui;
using Spectre.Console;
using System;
using System.Diagnostics;
using System.Text;

namespace QoreDB.Tui.Repl
{
    /// <summary>
    /// Encapsulates the main Read-Eval-Print Loop (REPL) for the QoreDB shell.
    /// </summary>
    public class QoreDbRepl
    {
        private Database _db;
        private readonly CommandProcessor _commandProcessor;

        public QoreDbRepl()
        {
            _db = new Database();
            _commandProcessor = new CommandProcessor();
        }

        /// <summary>
        /// Starts and runs the interactive shell.
        /// </summary>
        public void Run()
        {
            PrintWelcomeMessage();

            ReadLine.HistoryEnabled = true;
            ReadLine.AutoCompletionHandler = new SqlAutoCompleteHandler(_db);

            while (true)
            {
                var input = ReadCompleteStatement();

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                ReadLine.AddHistory(input);

                if (input.StartsWith("\\"))
                {
                    if (_commandProcessor.Handle(input, ref _db))
                    {
                        break; // Exit loop
                    }
                }
                else
                {
                    ExecuteSql(input);
                }
            }

            AnsiConsole.MarkupLine("[green]Goodbye![/]");
        }

        /// <summary>
        /// Reads lines from the console until a complete SQL statement (ending in ';')
        /// or a meta-command is entered.
        /// </summary>
        /// <returns>The complete, multi-line statement.</returns>
        private string ReadCompleteStatement()
        {
            var statementBuilder = new StringBuilder();

            // Read the first line with the primary prompt
            string firstLine = ReadLine.Read(GetReplName());

            // Meta-commands are executed immediately and are single-line only
            if (firstLine.Trim().StartsWith("\\"))
            {
                return firstLine;
            }

            statementBuilder.AppendLine(firstLine);

            // If the first line is already a complete statement, return it
            if (firstLine.TrimEnd().EndsWith(";"))
            {
                return statementBuilder.ToString();
            }

            // Read continuation lines until a semicolon is found
            while (true)
            {
                string continuationLine = ReadLine.Read("  -> ");
                statementBuilder.AppendLine(continuationLine);

                if (continuationLine.TrimEnd().EndsWith(";"))
                {
                    break;
                }
            }

            return statementBuilder.ToString();
        }

        private void ExecuteSql(string sql)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var result = _db.Execute(sql, 1000).Materialize();
                stopwatch.Stop();

                var printer = QueryResultPrinterFactory.GetPrinter(_db, result);
                printer.Print(result, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            }
        }

        private void PrintWelcomeMessage()
        {
            AnsiConsole.Write(
                new FigletText("QoreDB")
                    .Centered()
                    .Color(Color.Purple));

            AnsiConsole.MarkupLine("[grey]Welcome to the QoreDB interactive shell.[/]");
            AnsiConsole.MarkupLine("[grey]Use '[yellow]\\help[/]' or '[yellow]\\?[/]' for help.[/]");
        }

        private string GetReplName()
        {
            var dev = string.Empty;

            if (_db.DeveloperMode)
            {
                dev = $" (Dev: QEPO={_db.QueryOptimizationMode})";
            }

            return $"qore{dev}> ";
        }
    }
}