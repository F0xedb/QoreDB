# The Query Engine

The Query Engine is the heart of QoreDB, responsible for taking a raw SQL string and turning it into a result set. This process involves several key stages: parsing, optimization, and execution.

## Overview

The flow of a query through the engine is as follows:

1.  **Parsing**: The raw SQL query is converted into a structured representation called an Abstract Syntax Tree (AST).
2.  **Optimization**: The AST is transformed to create a more efficient execution plan.
3.  **Execution**: The optimized plan is executed to produce the final result.

This section provides a detailed look at each of these components.