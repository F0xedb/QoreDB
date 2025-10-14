using QoreDB.Tui.Repl;

namespace QoreDB.Tui
{
    public class Program
    {
        static void Main(string[] args)
        {
            var repl = new QoreDbRepl();
            repl.Run();
        }
    }
}