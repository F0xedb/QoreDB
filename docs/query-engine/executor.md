# Query Execution

The final stage in the query engine is execution. The execution engine takes the optimized execution plan from the optimizer and runs it to produce the query result.

## The Iterator (Volcano) Model

QoreDB's execution engine is built using the **Iterator model**, also known as the **Volcano model**. In this model, each operator in the execution plan is implemented as an iterator. Each iterator has a `Next()` method that returns the next tuple (row) in the result set.

### How it Works

- **Tree of Iterators**: The execution plan is a tree of these iterator operators. For example, a `SELECT` query might have a `TableScan` operator at the bottom, which reads rows from a table. This might be followed by a `Filter` operator, and then a `Projection` operator at the top.

- **Pull-Based Model**: The client (e.g., the TUI shell) calls `Next()` on the root operator of the tree. This operator, in turn, calls `Next()` on its children to get the data it needs, and this process continues down the tree. This is known as a "pull-based" model, as operators "pull" data from their children.

## Key Execution Operators

- **`TableScanOperator`**: Reads all the rows from a table.
- **`FilterOperator`**: Filters rows based on a `WHERE` clause condition.
- **`ProjectionOperator`**: Selects specific columns from the rows.
- **`SortOperator`**: Sorts the rows based on an `ORDER BY` clause.
- **`InsertOperator`**: Inserts new rows into a table.
- **`CreateTableOperator`**: Creates a new table.
- **`DropTableOperator`**: Deletes a table.

This iterator-based approach is highly extensible. To add a new SQL feature, you can simply implement a new operator and integrate it into the execution tree.