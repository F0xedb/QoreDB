# SQL Parser

The first step in processing a SQL query is parsing. The parser's job is to take a raw SQL string and convert it into a structured, hierarchical representation known as an Abstract Syntax Tree (AST).

## ANTLR: The Parser Generator

QoreDB uses **ANTLR (ANother Tool for Language Recognition)** to generate its SQL parser. ANTLR is a powerful parser generator that takes a grammar file as input and generates the necessary C# code to parse the language described by that grammar.

### The SQL Grammar (`Sql.g4`)

The grammar for QoreDB's supported SQL dialect is defined in the `Sql.g4` file. This file contains a set of rules that describe the syntax of the language, including statements, expressions, and keywords.

When the project is built, the ANTLR tool processes this grammar file and generates the lexer and parser classes that are used by the query engine.

## The Parsing Process

1.  **Lexical Analysis (Lexing)**: The input SQL string is broken down into a sequence of tokens. For example, the query `SELECT Name FROM Users` would be tokenized into `SELECT`, `Name`, `FROM`, `Users`.

2.  **Syntactic Analysis (Parsing)**: The sequence of tokens is then analyzed to ensure it conforms to the SQL grammar. If the syntax is valid, the parser builds an AST.

## The Abstract Syntax Tree (AST)

The AST is a tree-like data structure that represents the syntactic structure of the SQL query. Each node in the tree represents a construct in the query. For example, a `SELECT` statement would be represented by a `SelectNode` in the tree, with children representing the columns, table, and `WHERE` clause.

This AST is then passed on to the next stage of the query engine: the optimizer.