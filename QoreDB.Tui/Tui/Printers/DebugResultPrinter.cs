using QoreDB.QueryEngine.Execution.Models;
using QoreDB.QueryEngine.Interfaces;
using Spectre.Console;
using System;
using System.Linq;

namespace QoreDB.Tui.Tui.Printers
{
    /// <summary>
    /// A printer strategy for handling the detailed debug query result.
    /// </summary>
    public class DebugResultPrinter : IResultPrinter
    {
        private readonly Database _database;
        private readonly IResultPrinter _standardPrinter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugResultPrinter"/> class.
        /// </summary>
        /// <param name="standardPrinter">The printer to use for the underlying query result.</param>
        public DebugResultPrinter(Database db, IResultPrinter standardPrinter)
        {
            _database = db;
            _standardPrinter = standardPrinter;
        }

        public void Print(IQueryResult result, TimeSpan duration)
        {
            if (result is not DebugQueryResult debugResult)
            {
                // Fallback in case of incorrect usage
                _standardPrinter.Print(result, duration);
                return;
            }

            AnsiConsole.MarkupLine("[bold yellow]-- DEBUG MODE --[/]");

            PrintExecutionPlan(debugResult.ExecutionPlan, duration, _database.QueryOptimizationMode);
            PrintSummaryChart(debugResult.ExecutionPlan);
            PrintPagerStats(debugResult);

            AnsiConsole.MarkupLine("\n[bold yellow]-- QUERY RESULT --[/]");
            _standardPrinter.Print(debugResult.OriginalResult, duration);
        }
        
        private void PrintSummaryChart(IExecutionOperator plan)
        {
            var totalTime = plan.ExecutionTime.TotalMilliseconds;
            if (totalTime == 0) return;

            var breakdown = new BreakdownChart().Width(80);
            var nodes = plan.Flatten(); 

            var summary = nodes
                .GroupBy(op => op.Name.Split('(')[0].Trim())
                .Select(g => new
                {
                    Name = g.Key,
                    TotalDelta = g.Sum(op => op.GetDelta().TotalMilliseconds)
                })
                .OrderByDescending(x => x.TotalDelta);

            var colors = new[] { Color.Red, Color.Orange1, Color.Yellow, Color.Green, Color.Blue, Color.Purple };
            int colorIndex = 0;

            foreach (var item in summary)
            {
                breakdown.AddItem(item.Name, item.TotalDelta, colors[colorIndex++ % colors.Length]);
            }

            AnsiConsole.Write(
                new Panel(breakdown)
                {
                    Header = new PanelHeader("[bold]Query Profile by Operator[/]"),
                    Border = BoxBorder.Rounded,
                    Padding = new Padding(2, 1)
                });
        }

        private void PrintExecutionPlan(IExecutionOperator plan, TimeSpan duration, bool optimized)
        {
            var optimizedStr = optimized ? "(optimized) " : "";

            var table = new Table
            {
                Title = new TableTitle($"Execution Plan {optimizedStr}({duration.TotalMilliseconds:F2}ms)"),
                Border = TableBorder.Rounded
            };

            table.AddColumn("Operator");
            table.AddColumn("Time Spent (Δ)");

            var nodes = plan.Flatten();
            var maxDelta = nodes.Max(op => op.GetDelta().TotalMilliseconds);
            if (maxDelta == 0) maxDelta = 1;

            foreach (var op in nodes)
            {
                var indent = new string(' ', op.GetDepth() * 2);
                var connector = op.GetDepth() > 0 ? "└─ " : "";
                var opName = $"{indent}{connector}[yellow]{op.Name}[/]";

                var delta = op.GetDelta().TotalMilliseconds;
                int barWidth = (int)((delta / maxDelta) * 20);
                string bar = new string('▇', barWidth > 0 ? barWidth : 0);

                table.AddRow(opName, $"[cyan]{delta:F2}ms[/] [red]{bar}[/]");
            }
            AnsiConsole.Write(table);
        }

        private void PrintPagerStats(DebugQueryResult result)
        {
             AnsiConsole.WriteLine();
            var onDiskPages = result.PageCount - result.CachedPages;

            AnsiConsole.Write(new BreakdownChart()
                .Width(60)
                .AddItem("Cached Pages", result.CachedPages, Color.Green)
                .AddItem("On-Disk Pages", onDiskPages, Color.Grey));

            var statsTable = new Table().NoBorder().Collapse();
            statsTable.AddColumn("");
            statsTable.AddColumn("");
            statsTable.AddRow("[bold]File Size:[/]", $"{result.FileSize / 1024.0:F2} KB");
            statsTable.AddRow("[bold]Total Pages:[/]", $"{result.PageCount}");
            AnsiConsole.Write(statsTable);
            AnsiConsole.WriteLine();
        }
    }
}