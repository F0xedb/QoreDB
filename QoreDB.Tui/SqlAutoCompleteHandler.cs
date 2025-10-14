using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace QoreDB.Tui
{
    /// <summary>
    /// A sophisticated, context-aware autocompletion handler for both meta-commands and SQL.
    /// </summary>
    public class SqlAutoCompleteHandler : IAutoCompleteHandler
    {
        private readonly Database _db;
        private static readonly string[] _sqlKeywords = {
            "SELECT", "FROM", "WHERE", "ORDER BY", "LIMIT", "OFFSET",
            "CREATE TABLE", "INSERT INTO", "VALUES", "DROP TABLE",
            "ASC", "DESC"
        };
        private static readonly string[] _metaCommands = {
            "\\btree", "\\debug", "\\d", "\\dt", "\\exit", "\\help",
            "\\import", "\\info", "\\new", "\\q", "\\optimize"
        };

        public char[] Separators { get; set; } = new[] { ' ', ';', '(', ')' };

        public SqlAutoCompleteHandler(Database db)
        {
            _db = db;
        }

        /// <summary>
        /// Provides completion suggestions based on the current text and cursor position.
        /// </summary>
        public string[] GetSuggestions(string text, int index)
        {
            var currentWord = GetCurrentWord(text, index);

            // Handle meta-command completion first
            if (text.TrimStart().StartsWith("\\"))
            {
                return GetMetaCommandCompletions(text.TrimStart(), currentWord);
            }

            // Otherwise, handle SQL completion
            return GetSqlCommandCompletions(text, currentWord);
        }

        /// <summary>
        /// Gets the word at the current cursor position. (CORRECTED)
        /// </summary>
        private string GetCurrentWord(string text, int index)
        {
            if (string.IsNullOrEmpty(text) || index > text.Length)
            {
                return string.Empty;
            }

            // Find the start of the word by moving backward from the cursor
            var start = index - 1;
            while (start >= 0 && !Separators.Contains(text[start]))
            {
                start--;
            }
            start++;

            // Find the end of the word by moving forward from the start position
            var end = start;
            while (end < text.Length && !Separators.Contains(text[end]))
            {
                end++;
            }
            
            // If the cursor is not within the word found, we are at a separator, so the current "word" is empty.
            if (index < start || index > end)
            {
                return string.Empty;
            }
            
            return text.Substring(start, end - start);
        }


        /// <summary>
        /// Provides context-aware completions for meta-commands.
        /// </summary>
        private string[] GetMetaCommandCompletions(string text, string currentWord)
        {
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // If only typing the command itself
            if (parts.Length == 0 || (parts.Length == 1 && !text.EndsWith(" ")))
            {
                return _metaCommands.Where(c => c.StartsWith(currentWord)).ToArray();
            }

            // Context-aware completion for specific commands like \import
            return parts[0] switch
            {
                "\\import" => GetPathCompletions(currentWord),
                "\\d" or "\\dt" or "\\btree" => GetTableCompletions(currentWord),
                _ => Array.Empty<string>()
            };
        }

        /// <summary>
        /// Provides filesystem path completions for commands like \import.
        /// </summary>
        private string[] GetPathCompletions(string partialPath)
        {
            try
            {
                var directory = Path.GetDirectoryName(partialPath);
                var searchPattern = Path.GetFileName(partialPath);

                if (string.IsNullOrEmpty(directory))
                {
                    directory = "."; // Current directory
                }

                if (!Directory.Exists(directory))
                {
                    return Array.Empty<string>();
                }

                var fullDirectoryPath = Path.GetFullPath(directory);
                
                var directoryName = string.Empty;
                
                if (directory != ".")
                    directoryName = Path.GetFileName(directory);

                var dirs = Directory.GetDirectories(fullDirectoryPath)
                    .Select(Path.GetFileName)
                    .Where(d => d.StartsWith(searchPattern, StringComparison.OrdinalIgnoreCase))
                    .Select(d => Path.Combine(directoryName, d).Replace('\\', '/') + "/");

                var files = Directory.GetFiles(fullDirectoryPath)
                    .Select(Path.GetFileName)
                    .Where(f => f.StartsWith(searchPattern, StringComparison.OrdinalIgnoreCase))
                    .Select(f => Path.Combine(directoryName, f).Replace('\\', '/'));

                return dirs.Concat(files).ToArray();
            }
            catch
            {
                return Array.Empty<string>(); // Suppress errors during completion
            }
        }

        /// <summary>
        /// Provides context-aware completions for SQL statements.
        /// </summary>
        private string[] GetSqlCommandCompletions(string text, string currentWord)
        {
            var upperText = text.ToUpperInvariant();
            var lastKeyword = GetLastKeyword(upperText);

            switch (lastKeyword)
            {
                case "FROM":
                case "TABLE": // For DROP TABLE and CREATE TABLE
                case "INTO":  // For INSERT INTO
                    return GetTableCompletions(currentWord);

                case "SELECT":
                case "WHERE":
                case "BY": // For ORDER BY
                    var tableNames = GetTablesFromQuery(upperText);
                    return GetColumnCompletions(tableNames, currentWord)
                           .Concat(GetKeywordCompletions(currentWord))
                           .ToArray();

                default:
                    return GetKeywordCompletions(currentWord)
                           .Concat(GetTableCompletions(currentWord))
                           .ToArray();
            }
        }

        /// <summary>
        /// Finds the last major SQL keyword in the query to determine context.
        /// </summary>
        private string GetLastKeyword(string upperText)
        {
            // We need to handle multi-word keywords like "ORDER BY"
            var textToSearch = upperText.Replace("ORDER BY", "ORDERBY")
                                        .Replace("INSERT INTO", "INSERTINTO")
                                        .Replace("CREATE TABLE", "CREATETABLE")
                                        .Replace("DROP TABLE", "DROPTABLE");
                                        
            var words = textToSearch.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            var lastWord = words.LastOrDefault(w => _sqlKeywords.Any(k => k.Replace(" ", "") == w));
            
            // Map back to the original keyword
            if (lastWord == "ORDERBY") return "BY";
            if (lastWord == "INSERTINTO") return "INTO";
            if (lastWord == "CREATETABLE" || lastWord == "DROPTABLE") return "TABLE";
            
            return lastWord ?? string.Empty;
        }

        /// <summary>
        /// Extracts table names mentioned after a FROM clause.
        /// </summary>
        private IEnumerable<string> GetTablesFromQuery(string upperText)
        {
            var fromIndex = upperText.IndexOf("FROM");
            if (fromIndex == -1) return Enumerable.Empty<string>();

            var afterFrom = upperText.Substring(fromIndex + 4);
            var words = afterFrom.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            var potentialTable = words.FirstOrDefault();
            
            // ToLowerInvariant because table names in GetTable are case-sensitive from the catalog
            if (potentialTable != null && _db.GetTable(potentialTable.ToLowerInvariant()) != null)
            {
                return new[] { potentialTable.ToLowerInvariant() };
            }
            return Enumerable.Empty<string>();
        }

        private string[] GetKeywordCompletions(string currentWord)
            => _sqlKeywords.Where(k => k.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase)).ToArray();

        private string[] GetTableCompletions(string currentWord)
            => _db.GetTables()
                  .Select(t => t.TableName)
                  .Where(n => n.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                  .ToArray();

        private string[] GetColumnCompletions(IEnumerable<string> tableNames, string currentWord)
        {
            if (!tableNames.Any())
            {
                return Array.Empty<string>();
            }

            return tableNames
                .SelectMany(tableName => _db.GetTable(tableName)?.Columns ?? Enumerable.Empty<QoreDB.Catalog.Models.ColumnInfo>())
                .Select(c => c.ColumnName)
                .Where(n => n.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToArray();
        }
    }
}