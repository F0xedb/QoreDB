# The Catalog Manager

The **Catalog Manager** (or **System Catalog**) is the component of QoreDB that manages all the metadata about the database itself. Think of it as the database's internal "phone book," containing information about all the tables, columns, and other database objects.

## Responsibilities

The primary responsibilities of the Catalog Manager include:

- **Schema Management**: Storing and retrieving information about the database schema. This includes:
    - **Tables**: The names of all tables in the database.
    - **Columns**: For each table, the names and data types of its columns.
- **Metadata for the Query Engine**: The Query Engine relies on the Catalog Manager to:
    - **Parse Queries**: To validate that tables and columns referenced in a query actually exist.
    - **Plan Queries**: To get information about tables (e.g., their size) that can be used by the query optimizer.

## How it Works

In QoreDB, the catalog itself is stored in special tables within the database. For example, there might be a `_tables` table that lists all the tables, and a `_columns` table that lists all the columns.

When a query like `CREATE TABLE` is executed, the Catalog Manager is responsible for adding new entries to these internal tables. When a query like `SELECT` is executed, the Query Engine consults the Catalog Manager to get the necessary schema information.

By centralizing all metadata management, the Catalog Manager plays a crucial role in the overall architecture of the database.