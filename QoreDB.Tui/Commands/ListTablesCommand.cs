using Spectre.Console;
using System;
using System.Diagnostics;
using System.Linq;

namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Lists all tables in the database.
    /// </summary>
    public class ListTablesCommand : BaseCommand
    {
        public override string Name => "\\dt";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "List all tables.";

        public override bool Execute(ref Database db, string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            var tables = db.GetTables().ToList();
            var totalSize = db.GetStorageFileSize;
            stopwatch.Stop();

            var table = new Table()
                .Title($"Tables in Database ({totalSize / 1024.0:F2} KB)")
                .AddColumn("Table Name")
                .AddColumn("Column Count");

            if (!tables.Any())
            {
                table.AddRow("[grey]No tables found[/]", "[grey]-[/]");
            }
            else
            {
                foreach (var t in tables.OrderBy(t => t.TableName))
                {
                    table.AddRow(t.TableName, t.Columns.Count.ToString());
                }
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[grey]Execution time: {stopwatch.Elapsed.TotalMilliseconds:F2}ms[/]");
            return false;
        }
    }
}