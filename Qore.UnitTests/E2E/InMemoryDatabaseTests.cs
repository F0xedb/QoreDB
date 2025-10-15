using FluentAssertions;
using NUnit.Framework;
using QoreDB;
using QoreDB.QueryEngine.Execution.Models;
using System;

namespace Qore.UnitTests
{
    [TestFixture]
    public class InMemoryDatabaseTests
    {
        private Database _db;

        [SetUp]
        public void SetUp()
        {
            // A new, clean in-memory database is created for each test
            _db = new Database();
        }

        // =================================================================
        // DDL Statement Tests (CREATE, DROP)
        // =================================================================

        [Test]
        public void Execute_ValidCreateTable_Succeeds()
        {
            // Act
            var result = _db.Execute("CREATE TABLE Users (Id INT, Name STRING);") as MessageQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Table 'Users' created successfully");
        }

        [Test]
        public void Execute_CreateTableWhenTableExists_ThrowsException()
        {
            // Arrange
            _db.Execute("CREATE TABLE Users (Id INT);");

            // Act
            Action act = () => _db.Execute("CREATE TABLE Users (Id INT);");

            // Assert
            act.Should().Throw<Exception>().WithMessage("Table 'Users' already exists");
        }

        [Test]
        public void Execute_ValidDropTable_Succeeds()
        {
            // Arrange
            _db.Execute("CREATE TABLE Products (Sku STRING);");

            // Act
            var result = _db.Execute("DROP TABLE Products;") as MessageQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("Table 'Products' dropped successfully");
        }

        [Test]
        public void Execute_DropTableWhenTableDoesNotExist_ThrowsException()
        {
            // Act
            Action act = () => _db.Execute("DROP TABLE NonExistent;");

            // Assert
            act.Should().Throw<Exception>().WithMessage("Table 'NonExistent' not found");
        }

        // =================================================================
        // DML Statement Tests (INSERT, SELECT)
        // =================================================================

        [Test]
        public void Execute_ValidInsert_Succeeds()
        {
            // Arrange
            _db.Execute("CREATE TABLE Users (Id INT, Name STRING);");

            // Act
            var result = _db.Execute("INSERT INTO Users (Id, Name) VALUES (101, 'Alice');") as MessageQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Message.Should().Be("1 row inserted");
        }

        [Test]
        public void Execute_InsertIntoNonExistentTable_ThrowsException()
        {
            // Act
            Action act = () => _db.Execute("INSERT INTO NonExistent (Id) VALUES (1);");

            // Assert
            act.Should().Throw<Exception>().WithMessage("Table 'NonExistent' not found");
        }

        [Test]
        public void Execute_SelectFromEmptyTable_ReturnsZeroRows()
        {
            // Arrange
            _db.Execute("CREATE TABLE Users (Id INT, Name STRING);");

            // Act
            var result = _db.Execute("SELECT Id, Name FROM Users;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().BeEmpty();
        }

        // TODO: A full test for SELECT with data would need to be written
        // For now, this test validates the pipeline.
        [Test]
        public void Execute_SelectWithProjection_ReturnsCorrectStructure()
        {
            // Arrange
            _db.Execute("CREATE TABLE Users (Id INT, Name STRING, Age INT);");

            // Insert data here once select TableScanOperator is implemented
            // _db.Execute("INSERT INTO Users (Id, Name, Age) VALUES (1, 'Bob', 30);");

            // Act
            var result = _db.Execute("SELECT Name, Id FROM Users;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().BeEmpty();
        }

        [Test]
        public void Execute_SelectFromTableWithData_ReturnsAllRows()
        {
            // Arrange
            _db.Execute("CREATE TABLE Users (Id INT, Name STRING);");
            _db.Execute("INSERT INTO Users (Id, Name) VALUES (1, 'Alice');");
            _db.Execute("INSERT INTO Users (Id, Name) VALUES (2, 'Bob');");

            // Act
            var result = _db.Execute("SELECT Id, Name FROM Users;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
        }

        [Test]
        public void Execute_SelectWithWhereClause_ReturnsFilteredRows()
        {
            // Arrange
            _db.Execute("CREATE TABLE Employees (Id INT, City STRING);");
            _db.Execute("INSERT INTO Employees (Id, City) VALUES (1, 'Seattle');");
            _db.Execute("INSERT INTO Employees (Id, City) VALUES (2, 'New York');");
            _db.Execute("INSERT INTO Employees (Id, City) VALUES (3, 'Seattle');");

            // Act
            var result = _db.Execute("SELECT Id FROM Employees WHERE City = 'Seattle';") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
            result.Rows.Select(r => r["Id"]).Should().Contain(1).And.Contain(3);
        }

        [Test]
        public void Execute_SelectWithOrderByClause_ReturnsSortedRows()
        {
            // Arrange
            _db.Execute("CREATE TABLE Products (Name STRING, Price INT);");
            _db.Execute("INSERT INTO Products (Name, Price) VALUES ('B', 20);");
            _db.Execute("INSERT INTO Products (Name, Price) VALUES ('C', 10);");
            _db.Execute("INSERT INTO Products (Name, Price) VALUES ('A', 30);");

            // Act
            var result = _db.Execute("SELECT Name FROM Products ORDER BY Name ASC;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(3);
            result.Rows.Select(r => r["Name"]).Should().BeInAscendingOrder();
        }

        [Test]
        public void Execute_SelectWithWhereAndOrderBy_ReturnsFilteredAndSortedRows()
        {
            // Arrange
            _db.Execute("CREATE TABLE Students (Id INT, Name STRING, Score INT);");
            _db.Execute("INSERT INTO Students (Id, Name, Score) VALUES (1, 'Zoe', 85);");
            _db.Execute("INSERT INTO Students (Id, Name, Score) VALUES (2, 'Aaron', 95);");
            _db.Execute("INSERT INTO Students (Id, Name, Score) VALUES (3, 'Yara', 70);");
            _db.Execute("INSERT INTO Students (Id, Name, Score) VALUES (4, 'David', 95);");

            // Act
            var result = _db.Execute("SELECT Name FROM Students WHERE Score = 95 ORDER BY Name ASC;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
            result.Rows.Select(r => r["Name"]).Should().ContainInOrder("Aaron", "David");
        }

        [Test]
        public void Execute_SelectWithGreaterThanClause_ReturnsFilteredRows()
        {
            // Arrange
            _db.Execute("CREATE TABLE Products (Id INT, Price INT);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (1, 50);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (2, 100);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (3, 150);");

            // Act
            var result = _db.Execute("SELECT Id FROM Products WHERE Price > 75;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
            result.Rows.Select(r => r["Id"]).Should().Contain(2).And.Contain(3);
        }

        [Test]
        public void Execute_SelectLimit_ReturnsFilteredRows()
        {
            // Arrange
            _db.Execute("CREATE TABLE Products (Id INT, Price INT);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (1, 50);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (2, 100);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (3, 150);");

            // Act
            var result = _db.Execute("SELECT Id FROM Products LIMIT 2;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
            result.Rows.Select(r => r["Id"]).Should().Contain(1).And.Contain(2);
        }

        [Test]
        public void Execute_SelectLimit_ReturnsOneRow()
        {
            // Arrange
            _db.Execute("CREATE TABLE Products (Id INT, Price INT);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (1, 50);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (2, 100);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (3, 150);");

            // Act
            var result = _db.Execute("SELECT Id FROM Products LIMIT 1;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(1);
            result.Rows.Select(r => r["Id"]).Should().Contain(1);
        }

        [Test]
        public void Execute_SelectLimitWithOffset_ReturnsFilteredRows()
        {
            // Arrange
            _db.Execute("CREATE TABLE Products (Id INT, Price INT);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (1, 50);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (2, 100);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (3, 150);");

            // Act
            var result = _db.Execute("SELECT Id FROM Products LIMIT 2 OFFSET 1;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
            result.Rows.Select(r => r["Id"]).Should().Contain(2).And.Contain(3);
        }

        [Test]
        public void Execute_SelectWhereWithLimit_ReturnsFilteredRows()
        {
            // Arrange
            _db.Execute("CREATE TABLE Products (Id INT, Price INT);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (1, 50);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (2, 100);");
            _db.Execute("INSERT INTO Products (Id, Price) VALUES (3, 150);");

            // Act
            var result = _db.Execute("SELECT Id FROM Products WHERE Price > 0 LIMIT 2 OFFSET 1;") as RowsQueryResult;

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveCount(2);
            result.Rows.Select(r => r["Id"]).Should().Contain(2).And.Contain(3);
        }

        // =================================================================
        // Parser and Syntax Error Tests
        // =================================================================

        [Test]
        public void Execute_InvalidSqlSyntax_ThrowsArgumentException()
        {
            // Act
            Action act = () => _db.Execute("CREATE TABEL Users (Id INT);");

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Invalid SQL syntax: *");
        }

        [Test]
        public void Execute_InsertWithMismatchedColumnsAndValues_ThrowsInvalidOperationException()
        {
            // Arrange
            _db.Execute("CREATE TABLE Users (Id INT, Name STRING);");

            // Act
            Action act = () => _db.Execute("INSERT INTO Users (Id) VALUES (1, 'Alice');");

            // Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("Column count does not match value count");
        }


        [Test]
        public void Execute_SelectWithInvalidWhereClause_ThrowsArgumentException()
        {
            // Act
            Action act = () => _db.Execute("SELECT Id FROM Users WHERE Id <>;");

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Invalid SQL syntax: *");
        }

        [Test]
        public void Execute_SelectWithInvalidOrderByClause_ThrowsArgumentException()
        {
            // Act
            Action act = () => _db.Execute("SELECT Id FROM Users ORDER Id;");

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Invalid SQL syntax: *");
        }
    }
}