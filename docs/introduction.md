# Introduction to QoreDB

Welcome to the official documentation for QoreDB, a lightweight, educational SQL database engine built from the ground up in C#. This project serves as a practical exploration into the internal workings of a relational database management system (RDBMS).

## What is QoreDB?

QoreDB is an educational tool designed to demystify the core components of a database. It provides a hands-on learning experience for anyone interested in database design, query processing, and data storage. By building this project, we aim to provide a clear and understandable implementation of complex database concepts.

### Key Learning Objectives

- **Database Architecture**: Understand the layered architecture of a typical database system, from the user interface to the physical storage layer.
- **SQL Parsing and Execution**: Learn how raw SQL queries are parsed into an Abstract Syntax Tree (AST) and then executed by a query engine.
- **Query Optimization**: Explore techniques for optimizing query execution plans to improve performance.
- **Data Storage and Indexing**: Delve into the implementation of a B+ Tree, a fundamental data structure for efficient data retrieval in databases.
- **Schema Management**: Understand how a database manages metadata about tables, columns, and data types through a system catalog.

## Who is this for?

This project is intended for:

- **Students**: Computer science students studying database systems.
- **Developers**: Software developers who want to deepen their understanding of how databases work under the hood.
- **Educators**: Instructors looking for a practical example to accompany their database curriculum.
- **Hobbyists**: Anyone with a passion for learning and building complex systems.

## Project Philosophy

QoreDB is built with the following principles in mind:

- **Clarity over Performance**: While performance is important, the primary goal of this project is to provide a clear and readable codebase.
- **Modularity**: The project is divided into distinct layers, making it easy to understand and extend each component independently.
- **From Scratch**: We avoid using high-level libraries for core database functionality to provide a deeper insight into the implementation details.

We hope you find QoreDB to be a valuable learning resource. Let's get started.