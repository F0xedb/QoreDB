-- seed.sql
-- A sample seed file for QoreDB.

-- Drop existing tables to ensure a clean slate, in reverse order of creation
DROP TABLE IF EXISTS Users;

-- Create a Users table
CREATE TABLE Users ( ID INT, Name STRING );

INSERT INTO Users (ID, Name) VALUES (1001, 'Tom');
INSERT INTO Users (ID, Name) VALUES (1002, 'John');