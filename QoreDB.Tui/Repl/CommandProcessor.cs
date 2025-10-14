using QoreDB.Tui.Commands;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoreDB.Tui.Repl
{
    /// <summary>
    /// Discovers, manages, and executes meta-commands.
    /// </summary>
    public class CommandProcessor
    {
        private readonly Dictionary<string, ICommand> _commands = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandProcessor"/> class
        /// and discovers all ICommand implementations in the assembly.
        /// </summary>
        public CommandProcessor()
        {
            var commandTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(ICommand).IsAssignableFrom(t))
                .ToList();

            var otherCommands = new List<ICommand>();
            foreach (var type in commandTypes.Where(t => t != typeof(HelpCommand)))
            {
                if (Activator.CreateInstance(type) is ICommand command)
                {
                    otherCommands.Add(command);
                }
            }

            var helpCommand = new HelpCommand(otherCommands);
            
            var allCommands = otherCommands.Concat(new[] { helpCommand });
            foreach (var command in allCommands)
            {
                _commands.Add(command.Name, command);
                foreach (var alias in command.Aliases)
                {
                    _commands.Add(alias, command);
                }
            }
        }

        /// <summary>
        /// Gets all unique commands.
        /// </summary>
        public IEnumerable<ICommand> Commands => _commands.Values.Distinct();

        /// <summary>
        /// Handles a meta-command string input.
        /// </summary>
        /// <param name="line">The raw input line from the user.</param>
        /// <param name="db">The current database instance.</param>
        /// <returns>True if the application should exit.</returns>
        public bool Handle(string line, ref Database db)
        {
            // TODO: Have a better command option parsing logic
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var cmdName = parts[0];

            if (_commands.TryGetValue(cmdName, out var command))
            {
                return command.Execute(ref db, parts.Skip(1).ToArray());
            }

            AnsiConsole.MarkupLine($"[red]Error: Unrecognized command '[yellow]{cmdName}[/]'.[/]");
            return false;
        }
    }
}