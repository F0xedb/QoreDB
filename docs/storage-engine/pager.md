# The Pager

The Pager is a critical component of the storage engine, acting as an intermediary between the database's higher-level components and the physical storage on disk.

## Page-Based Architecture

Databases typically manage data in fixed-size blocks called **pages**. All disk I/O is done in units of pages. This approach has several advantages:

- **Efficient I/O**: Reading a single large block from disk is more efficient than reading many small, scattered pieces of data.
- **Simplified Memory Management**: The buffer pool in memory can be managed as an array of page-sized slots.

In QoreDB, the page size is a constant, typically 4KB.

## Responsibilities of the Pager

- **Reading and Writing Pages**: The pager provides an API for reading a specific page from the database file into memory and writing a page from memory back to the file.
- **Buffer Pool Management**: To avoid a disk read every time a page is needed, the pager maintains a cache of pages in memory, known as the **buffer pool**. When a page is requested, the pager first checks if it's already in the buffer pool. If it is, a disk read is avoided.
- **Concurrency Control**: In a multi-user database, the pager would also be responsible for managing locks on pages to ensure data consistency. (Note: The current version of QoreDB is single-user).

## The QoreDB Pager

QoreDB includes two pager implementations:

- **`QorePager`**: A disk-based pager that manages a database file on disk.
- **`InMemoryQorePager`**: An in-memory pager used for testing and transient databases.

By abstracting away the details of disk I/O, the pager allows the rest of the database to work with data in a more abstract, page-oriented manner.