# Query Optimizer

Once a SQL query has been parsed into an Abstract Syntax Tree (AST), the next step is to optimize it. The goal of the query optimizer is to find the most efficient way to execute the query. A single SQL query can often be executed in many different ways, and the optimizer's job is to choose the best execution plan.

## Rule-Based Optimization

QoreDB uses a simple rule-based optimizer. It applies a series of predefined rules to the AST to transform it into a more efficient, but semantically equivalent, plan.

### Key Optimization Rules

- **Predicate Pushdown**: This is one of the most important optimization techniques. It involves moving filtering conditions (`WHERE` clauses) as far down the execution tree as possible. By filtering data early, we can significantly reduce the amount of data that needs to be processed by subsequent operators.

  For example, in a query with a `JOIN` and a `WHERE` clause, pushing the `WHERE` clause down to the table scan level means we filter the rows before they are joined, rather than joining large tables and then filtering the result.

- **Projection Pushdown**: This rule involves pushing down the selection of columns (`SELECT` clause) to the earliest possible stage. This reduces the amount of data that needs to be moved between operators.

- **Constant Folding**: This rule involves evaluating constant expressions at optimization time rather than at execution time. For example, the expression `WHERE Id = 10 + 5` would be transformed to `WHERE Id = 15`.

## The Optimization Process

The optimizer takes the initial AST from the parser and applies these rules iteratively until no more rules can be applied. The resulting transformed tree is the optimized execution plan, which is then passed to the execution engine.