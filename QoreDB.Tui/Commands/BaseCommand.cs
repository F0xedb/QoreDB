namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// A base class for commands to share common functionality if needed in the future.
    /// </summary>
    public abstract class BaseCommand : ICommand
    {
        public abstract string Name { get; }
        public abstract string[] Aliases { get; }
        public abstract string Description { get; }

        public abstract bool Execute(ref Database db, string[] args);
    }
}