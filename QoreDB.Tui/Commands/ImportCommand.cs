using Spectre.Console;
using System;
using System.Diagnostics;
using System.IO;

namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Imports and executes SQL statements from a file.
    /// </summary>
    public class ImportCommand : BaseCommand
    {
        public override string Name => "\\import";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Execute SQL commands from a file.";

        public override bool Execute(ref Database db, string[] args)
        {
            if (args.Length < 1)
            {
                AnsiConsole.MarkupLine("[red]Error: \\import command requires a file path.[/]");
                return false;
            }

            var filePath = args[0];
            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"[red]Error: File not found at '[yellow]{filePath}[/]'.[/]");
                return false;
            }

            AnsiConsole.MarkupLine($"[grey]Importing from '{filePath}'...[/]");
            var stopwatch = Stopwatch.StartNew();
            var fileContent = File.ReadAllText(filePath);

            var statements = fileContent.Split(';', StringSplitOptions.RemoveEmptyEntries);
            int successCount = 0;

            foreach (var stmt in statements)
            {
                var trimmedStmt = stmt.Trim();
                if (string.IsNullOrWhiteSpace(trimmedStmt)) continue;

                try
                {
                    db.Execute(trimmedStmt + ";");
                    successCount++;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error executing statement:[/] [grey]{trimmedStmt}[/]");
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
                    AnsiConsole.MarkupLine("[red]Import aborted.[/]");
                    return false; // Stop on first error
                }
            }

            stopwatch.Stop();
            AnsiConsole.MarkupLine($"[green]✅ Successfully executed {successCount} statement(s) in {stopwatch.Elapsed.TotalMilliseconds:F2}ms.[/]");
            return false;
        }
    }
}