namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Represents a meta-command that can be executed in the TUI.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the name of the command (e.g., "\\q").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a list of aliases for the command (e.g., "\\exit").
        /// </summary>
        string[] Aliases { get; }

        /// <summary>
        /// Gets a user-friendly description of what the command does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Executes the command's action.
        /// </summary>
        /// <param name="db">The current database instance.</param>
        /// <param name="args">The arguments passed to the command.</param>
        /// <returns>True if the application should exit, otherwise false.</returns>
        bool Execute(ref Database db, string[] args);
    }
}