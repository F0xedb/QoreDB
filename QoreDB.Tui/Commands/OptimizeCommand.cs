using Spectre.Console;
using System;

namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Toggles the query execution plan optimization mode.
    /// </summary>
    public class OptimizeCommand : BaseCommand
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public override string Name => "\\optimize";

        /// <summary>
        /// Gets the aliases for the command.
        /// </summary>
        public override string[] Aliases => Array.Empty<string>();

        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        public override string Description => "Toggle Query Execution Plan optimization mode.";

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="db">The database instance.</param>
        /// <param name="args">The arguments for the command.</param>
        /// <returns>A boolean indicating whether the REPL should exit.</returns>
        public override bool Execute(ref Database db, string[] args)
        {
            db.QueryOptimizationMode = !db.QueryOptimizationMode;
            AnsiConsole.MarkupLine(db.QueryOptimizationMode
                ? "[green]Query Execution Plan optimization mode enabled.[/]"
                : "[yellow]Query Execution Plan optimization mode disabled.[/]");
            return false;
        }
    }
}