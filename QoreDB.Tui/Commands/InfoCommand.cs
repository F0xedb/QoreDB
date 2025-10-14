using Spectre.Console;
using System;

namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Displays version and author information for the database.
    /// </summary>
    public class InfoCommand : BaseCommand
    {
        public override string Name => "\\info";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Display information about the database.";

        public override bool Execute(ref Database db, string[] args)
        {
            var table = new Table().NoBorder().HideHeaders();
            table.AddColumn("");
            table.AddColumn("");

            table.AddRow("[bold]Name[/]", VersionInfo.Name);
            table.AddRow("[bold]Version[/]", VersionInfo.Version);
            table.AddRow("[bold]Author[/]", VersionInfo.Author);

            var panel = new Panel(table)
            {
                Header = new PanelHeader($"[purple]{VersionInfo.Name} Information[/]"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(2, 1)
            };
            
            AnsiConsole.Write(panel);
            return false;
        }
    }
}