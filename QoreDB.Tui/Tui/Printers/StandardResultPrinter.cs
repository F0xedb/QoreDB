using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;
using Spectre.Console;
using System;
using System.Linq;

namespace QoreDB.Tui.Tui.Printers
{
    /// <summary>
    /// A printer strategy for handling standard query results (messages and row sets).
    /// </summary>
    public class StandardResultPrinter : IResultPrinter
    {
        public void Print(IQueryResult result, TimeSpan duration)
        {
            if (result is MessageQueryResult msg)
            {
                AnsiConsole.MarkupLine($"[green]✅ {msg.Message}[/]");
            }
            else if (result is RowsQueryResult rowsResult)
            {
                PrintRowsResult(rowsResult);
            }

            PrintDuration(duration);
        }

        private void PrintRowsResult(RowsQueryResult result)
        {
            var rows = result.Rows.ToList();
            if (!rows.Any())
            {
                AnsiConsole.MarkupLine("[yellow](0 rows returned)[/]");
                return;
            }

            var table = new Table().Expand();
            var headers = rows.First().Keys.ToList();

            foreach (var header in headers)
            {
                table.AddColumn($"[bold]{header}[/]");
            }

            foreach (var row in rows)
            {
                var rowData = headers.Select(h => row[h]?.ToString() ?? "[grey]NULL[/]").ToArray();
                table.AddRow(rowData);
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[dim]({rows.Count} row(s) returned)[/]");
        }

        private void PrintDuration(TimeSpan duration)
        {
            AnsiConsole.MarkupLine($"[grey]Execution time: {duration.TotalMilliseconds:F2}ms[/]");
        }
    }
}