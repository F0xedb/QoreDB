using Spectre.Console;
using System.Collections.Generic;
using System.Linq;

namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Displays the help message with all available commands.
    /// </summary>
    public class HelpCommand : ICommand
    {
        private readonly IEnumerable<ICommand> _allCommands;

        public string Name => "\\help";
        public string[] Aliases => new[] { "\\?" };
        public string Description => "Show this help message.";

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpCommand"/> class.
        /// </summary>
        /// <param name="allCommands">The collection of all commands to be displayed.</param>
        public HelpCommand(IEnumerable<ICommand> allCommands)
        {
            _allCommands = allCommands;
        }

        public bool Execute(ref Database db, string[] args)
        {
            var generalPanel = new Panel("[bold]General[/]\n  All SQL commands must end with a semicolon (;) to be executed.")
            {
                Border = BoxBorder.None,
                Padding = new Padding(2, 1, 0, 1) // No right padding
            };
            
            var commandsPanel = new Panel(BuildCommandsGrid())
            {
                Header = new PanelHeader("[bold]Meta-Commands[/]"),
                Border = BoxBorder.None,
                Padding = new Padding(2, 0, 0, 1) // No top/right padding
            };

            var layout = new Padder(new Rows(generalPanel, commandsPanel), new Padding(2, 1));

            var mainPanel = new Panel(layout)
            {
                Header = new PanelHeader("[purple]QoreDB Help[/]"),
                Border = BoxBorder.Rounded
            };
            
            AnsiConsole.Write(mainPanel);
            return false;
        }

        private Grid BuildCommandsGrid()
        {
            var grid = new Grid().Expand();
            grid.AddColumn(new GridColumn().PadRight(4)); // Column for commands
            grid.AddColumn(); // Column for descriptions

            // Create a combined list that includes this help command itself for display
            var displayCommands = _allCommands.Concat(new[] { this }).OrderBy(c => c.Name);

            foreach (var cmd in displayCommands)
            {
                // Build the command part with aliases
                var aliasText = cmd.Aliases.Any() ? $" or [yellow]{string.Join(", ", cmd.Aliases)}[/]" : "";
                var commandText = $"[yellow]{cmd.Name}[/]{aliasText}";

                // Add the row to the grid
                grid.AddRow(commandText, cmd.Description);
            }

            return grid;
        }
    }
}