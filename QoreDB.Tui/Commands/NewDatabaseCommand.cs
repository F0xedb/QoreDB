using QoreDB.StorageEngine;
using Spectre.Console;
using System;

namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Creates a new in-memory database with specified parameters.
    /// </summary>
    public class NewDatabaseCommand : BaseCommand
    {
        public override string Name => "\\new";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Create a new database with a given page size.";

        public override bool Execute(ref Database db, string[] args)
        {
            var degree = Constants.DEFAULT_BTREE_PAGE_DEGREE;
            var pageSize = Constants.DEFAULT_PAGE_SIZE;
            var cacheSize = Constants.DEFAULT_PAGE_CACHE_SIZE;

            if (args.Length < 3)
            {
                AnsiConsole.MarkupLine($"[red]Error: \\new command requires a btree degree, page size and cache size but only found {args.Length}.[/]");
                return false;
            }
            
            if (int.TryParse(args[0], out degree) && 
                int.TryParse(args[1], out pageSize) && 
                int.TryParse(args[2], out cacheSize))
            {
                db = new Database(degree, pageSize, cacheSize);
                AnsiConsole.MarkupLine("[green]New database created.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid arguments for \\new command. All parameters must be integers.[/]");
            }

            return false;
        }
    }
}