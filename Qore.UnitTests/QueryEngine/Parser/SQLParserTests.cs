using FluentAssertions;
using QoreDB.QueryEngine.Execution.Operators;
using QoreDB.QueryEngine.Parser;

namespace Qore.UnitTests.QueryEngine.Parser
{
    [TestFixture]
    public class SQLParserTests
    {
        private SQLParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new SQLParser();
        }

        [Test]
        public void Parse_ValidInsertStatement_BuildsCorrectPlan()
        {
            // Arrange
            var sql = "INSERT INTO Users (Id, Name) VALUES (123, 'John Doe');";

            // Act
            var plan = _parser.Parse(sql);
            var insertOp = plan.Root as InsertOperator;

            // Assert
            insertOp.Should().NotBeNull();
        }

        [Test]
        public void Parse_ValidSelectStatement_BuildsCorrectPlan()
        {
            // Arrange
            var sql = "SELECT Name, Age FROM Customers;";

            // Act
            var plan = _parser.Parse(sql);
            var projectionOp = plan.Root as ProjectionOperator;
            var tableScanOp = projectionOp?.Source as TableScanOperator;

            // Assert
            projectionOp.Should().NotBeNull();
            tableScanOp.Should().NotBeNull();
        }

        [Test]
        public void Parse_InvalidSqlSyntax_ThrowsArgumentException()
        {
            // Arrange
            var sql = "SELECT FROM Users WHERE;"; // Clearly invalid syntax

            // Act
            Action act = () => _parser.Parse(sql);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("Invalid SQL syntax: *");
        }

        [Test]
        public void Parse_ValidCreateTableStatement_BuildsCorrectPlan()
        {
            // Arrange
            var sql = "CREATE TABLE Employees (Id INT, Name STRING);";

            // Act
            var plan = _parser.Parse(sql);
            var createOp = plan.Root as CreateTableOperator;

            // Assert
            createOp.Should().NotBeNull();
        }

        [Test]
        public void Parse_ValidDropTableStatement_BuildsCorrectPlan()
        {
            // Arrange
            var sql = "DROP TABLE Products;";

            // Act
            var plan = _parser.Parse(sql);
            var dropOp = plan.Root as DropTableOperator;

            // Assert
            dropOp.Should().NotBeNull();
        }

        [Test]
        public void Parse_SelectWithWhereAndOrderBy_BuildsCorrectPlan()
        {
            // Arrange
            var sql = "SELECT Name FROM Users WHERE Id = 1 ORDER BY Name DESC;";

            // Act
            var plan = _parser.Parse(sql);
            
            // Assert: Check the operator chain
            var projectionOp = plan.Root.Should().BeOfType<ProjectionOperator>().Subject;
            var sortOp = projectionOp.Source.Should().BeOfType<SortOperator>().Subject;
            var filterOp = sortOp.Source.Should().BeOfType<FilterOperator>().Subject;
            filterOp.Source.Should().BeOfType<TableScanOperator>();
        }
    }
}