-- seed.sql
-- A sample seed file for QoreDB.

-- Drop existing tables to ensure a clean slate, in reverse order of creation
DROP TABLE IF EXISTS Orders;
DROP TABLE IF EXISTS Employees;
DROP TABLE IF EXISTS Departments;

-- Create a Departments table
CREATE TABLE Departments (Id INT, Name STRING );

-- Create an Employees table
CREATE TABLE Employees ( Id INT, Name STRING, City STRING, DepartmentId INT);

-- Create an Orders table
CREATE TABLE Orders ( OrderId INT, ProductName STRING, Quantity INT, EmployeeId INT);

CREATE TABLE Users ( Id INT, Name STRING );

-- Insert data into the Departments table
INSERT INTO Departments (Id, Name) VALUES (10, 'Engineering');
INSERT INTO Departments (Id, Name) VALUES (20, 'Sales');
INSERT INTO Departments (Id, Name) VALUES (30, 'HR');

-- Insert data into the Employees table
INSERT INTO Employees (Id, Name, City, DepartmentId) VALUES (101, 'Alice', 'Seattle', 10);
INSERT INTO Employees (Id, Name, City, DepartmentId) VALUES (102, 'Bob', 'New York', 20);
INSERT INTO Employees (Id, Name, City, DepartmentId) VALUES (103, 'Charlie', 'Seattle', 10);
INSERT INTO Employees (Id, Name, City, DepartmentId) VALUES (104, 'Diana', 'Chicago', 30);
INSERT INTO Employees (Id, Name, City, DepartmentId) VALUES (105, 'Evan', 'New York', 20);

-- Insert data into the Orders table
INSERT INTO Orders (OrderId, ProductName, Quantity, EmployeeId) VALUES (1001, 'Laptop', 5, 102);
INSERT INTO Orders (OrderId, ProductName, Quantity, EmployeeId) VALUES (1002, 'Mouse', 20, 105);
INSERT INTO Orders (OrderId, ProductName, Quantity, EmployeeId) VALUES (1003, 'Keyboard', 15, 102);
INSERT INTO Orders (OrderId, ProductName, Quantity, EmployeeId) VALUES (1004, 'Monitor', 10, 101);

INSERT INTO Users (Id, Name) VALUES (1001, 'Tom');
INSERT INTO Users (Id, Name) VALUES (1002, 'Tim');