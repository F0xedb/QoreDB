# Getting Started with QoreDB

This guide will walk you through the process of setting up and running QoreDB on your local machine.

## Prerequisites

Before you begin, ensure you have the following software installed:

* **.NET 9 SDK**: QoreDB is built using C# and the .NET 9 framework.
* **Java Runtime Environment (JRE)**: ANTLR, the parser generator used in this project, requires a JRE for code generation.

## Installation

1.  **Clone the Repository**

    First, clone the QoreDB repository to your local machine using Git.

    ```bash
    git clone <your-repo-url>
    cd QoreDB
    ```

2.  **Build the Project**

    Next, build the solution using the `dotnet build` command. This will also trigger the ANTLR code generation process, which creates the SQL parser from the grammar file.

    ```bash
    dotnet build
    ```

## Running the TUI Shell

Once the project is built, you can run the interactive Terminal User Interface (TUI) to start interacting with the database.

```bash
dotnet run --project QoreDB.Tui/QoreDB.Tui.csproj
```

This will launch the QoreDB shell, where you can execute SQL commands and meta-commands.

## Your First Interaction

Here's a quick example of how to create a table, insert data, and query it using the TUI shell:

1.  **Create a Table**

    ```sql
    CREATE TABLE Employees (Id INT, Name STRING, City STRING);
    ```

2.  **Insert Data**

    ```sql
    INSERT INTO Employees (Id, Name, City) VALUES (1, 'Alice', 'Seattle');
    INSERT INTO Employees (Id, Name, City) VALUES (2, 'Bob', 'New York');
    ```

3.  **Query Data**

    ```sql
    SELECT Name FROM Employees WHERE City = 'Seattle';
    ```

You should see the following output:

```
┌───────┐
│ Name  │
├───────┤
│ Alice │
└───────┘
```

Congratulations, you have successfully set up and used QoreDB.