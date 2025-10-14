namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Exits the QoreDB shell.
    /// </summary>
    public class ExitCommand : BaseCommand
    {
        public override string Name => "\\q";
        public override string[] Aliases => new[] { "\\exit" };
        public override string Description => "Exit the shell.";

        public override bool Execute(ref Database db, string[] args)
        {
            return true; // Signal to exit
        }
    }
}