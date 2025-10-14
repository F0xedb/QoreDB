using Spectre.Console;
using System;

namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Toggles the developer debug mode.
    /// </summary>
    public class DebugCommand : BaseCommand
    {
        public override string Name => "\\debug";
        public override string[] Aliases => new[] { "\\dev" };
        public override string Description => "Toggle developer debug mode.";

        public override bool Execute(ref Database db, string[] args)
        {
            db.DeveloperMode = !db.DeveloperMode;
            AnsiConsole.MarkupLine(db.DeveloperMode
                ? "[green]Dev mode enabled.[/]"
                : "[yellow]Dev mode disabled.[/]");
            return false;
        }
    }
}