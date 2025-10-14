# The B+ Tree

The **B+ Tree** is the primary data structure used in QoreDB for indexing and storing data. It's a type of balanced tree that is optimized for systems that read and write large blocks of data, making it ideal for database storage engines.

## Why a B+ Tree?

B+ Trees have several properties that make them well-suited for databases:

- **Balanced**: The tree is always balanced, which guarantees that the time to access any data is logarithmic in the number of entries.
- **High Fanout**: Each node in the tree can have a large number of children (a high "fanout"). This means the tree is very wide and shallow, reducing the number of disk reads required to find a piece of data.
- **Sorted Data**: All data is stored in the leaf nodes in sorted order, which is efficient for range-based queries (e.g., `WHERE age > 30`).
- **Sequential Access**: The leaf nodes are linked together in a linked list, allowing for efficient sequential scanning of all the data.

## Structure of a B+ Tree

A B+ Tree consists of two types of nodes:

- **Internal Nodes**: These nodes store keys and pointers to their child nodes. They are used to navigate the tree.
- **Leaf Nodes**: These nodes store the actual data (or pointers to the data). All leaf nodes are at the same level in the tree.

## B+ Tree Operations in QoreDB

The B+ Tree implementation in QoreDB supports the following operations:

- **`Insert`**: Inserts a new key-value pair into the tree. This may involve splitting nodes if they become full.
- **`Search`**: Searches for a key in the tree and returns the corresponding value.
- **`Delete`**: Deletes a key-value pair from the tree. This may involve merging nodes if they become under-full.

The B+ Tree is the core of the storage engine, providing efficient and reliable storage for all the data in the database.