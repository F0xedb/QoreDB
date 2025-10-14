using Spectre.Console;
using System;
using System.Diagnostics;

namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Describes the columns and types of a specific table.
    /// </summary>
    public class DescribeTableCommand : BaseCommand
    {
        public override string Name => "\\d";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Describe a table.";

        public override bool Execute(ref Database db, string[] args)
        {
            if (args.Length < 1)
            {
                AnsiConsole.MarkupLine("[red]Error: \\d command requires a table name.[/]");
                return false;
            }

            var tableName = args[0];
            var stopwatch = Stopwatch.StartNew();
            var tableInfo = db.GetTable(tableName);
            stopwatch.Stop();

            if (tableInfo == null)
            {
                AnsiConsole.MarkupLine($"[red]Error: Table '[yellow]{tableName}[/]' not found.[/]");
                return false;
            }

            var table = new Table()
                .Title($"Description of [yellow]{tableInfo.TableName}[/]")
                .AddColumn("Column")
                .AddColumn("Type");

            foreach (var col in tableInfo.Columns)
            {
                table.AddRow($"[bold]{col.ColumnName}[/]", $"[green]{col.DataType.Name}[/]");
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[grey]Execution time: {stopwatch.Elapsed.TotalMilliseconds:F2}ms[/]");
            return false;
        }
    }
}