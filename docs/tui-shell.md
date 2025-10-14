# The TUI Shell

The QoreDB Terminal User Interface (TUI) provides a simple yet powerful interactive shell for working with your database.

## Features

- **Interactive SQL Execution**: Write and execute SQL queries in real-time.
- **Meta-Commands**: Use special commands for database management tasks.
- **Command History**: Navigate through your previous commands using the arrow keys.
- **Auto-Completion**: Basic auto-completion for SQL keywords and table names.
- **Formatted Output**: Query results are displayed in a clean, readable table format, thanks to Spectre.Console.

## Meta-Commands

Meta-commands are special commands that are not part of the SQL language but are recognized by the TUI shell. They all start with a backslash (`\`).

| Command           | Description                                       |
| ----------------- | ------------------------------------------------- |
| `\help` or `\?`   | Displays a help message with all available commands. |
| `\dt`             | Lists all the tables in the current database.     |
| `\d <table>`      | Describes the columns of a specific table, showing their names and data types. |
| `\import <file>`  | Executes a series of SQL statements from a file.  |
| `\q` or `\exit`   | Exits the TUI shell.                              |

### Example Usage

- **Listing Tables:**

  ```
  \dt
  ```

- **Describing a Table:**

  ```
  \d Employees
  ```

- **Importing a SQL Script:**

  ```
  \import /path/to/your/script.sql
  ```

## Customizing the Shell

The TUI shell is built to be extensible. You can add new meta-commands by implementing the `ICommand` interface and registering them in the `CommandProcessor`. This allows for the addition of custom functionality tailored to your specific needs.