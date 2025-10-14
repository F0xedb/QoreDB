# SQL Reference

This document provides a reference for the SQL syntax and operations supported by QoreDB.

## Data Types

QoreDB supports the following data types:

- `INT`: A 32-bit signed integer.
- `STRING`: A variable-length string.

## Supported Operations

### `CREATE TABLE`

Creates a new table in the database.

**Syntax:**

```sql
CREATE TABLE table_name (
    column1_name column1_type,
    column2_name column2_type,
    ...
);
```

**Example:**

```sql
CREATE TABLE Users (
    Id INT,
    Username STRING,
    Email STRING
);
```

### `DROP TABLE`

Deletes an existing table from the database.

**Syntax:**

```sql
DROP TABLE table_name;
```

**Example:**

```sql
DROP TABLE Users;
```

### `INSERT INTO`

Inserts a new row of data into a table.

**Syntax:**

```sql
INSERT INTO table_name (column1, column2, ...)
VALUES (value1, value2, ...);
```

**Example:**

```sql
INSERT INTO Users (Id, Username, Email)
VALUES (1, 'john_doe', 'john.doe@example.com');
```

### `SELECT`

Retrieves data from one or more tables.

**Syntax:**

```sql
SELECT column1, column2, ... | *
FROM table_name
[WHERE condition]
[ORDER BY column_name [ASC | DESC]];
```

**Clauses:**

- **`SELECT ...`**: Specifies the columns to be returned. Use `*` to select all columns.
- **`FROM ...`**: Specifies the table to retrieve data from.
- **`WHERE ...`**: Filters rows based on a specified condition.
- **`ORDER BY ...`**: Sorts the result set in ascending (`ASC`) or descending (`DESC`) order.

**Supported Operators in `WHERE` Clause:**

- `=` (Equal to)
- `!=` or `<>` (Not equal to)
- `>` (Greater than)
- `<` (Less than)
- `>=` (Greater than or equal to)
- `<=` (Less than or equal to)

**Example:**

```sql
SELECT Username, Email
FROM Users
WHERE Id > 10
ORDER BY Username DESC;
```