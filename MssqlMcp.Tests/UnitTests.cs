using Mssql.McpServer;
using Mssql.McpServer.Common;

namespace MssqlMcp.Tests
{
    /// <summary>
    /// All tests in this class share a single database via the DatabaseFixture
    /// </summary>
    [Collection("Database collection")]
    public sealed class MssqlMcpTests : IDisposable
    {
        private readonly string _testDatabaseName;
        private readonly string _tableName;
        private readonly Tools _tools;

        public MssqlMcpTests(DatabaseFixture fixture)
        {
            _testDatabaseName = fixture.TestDatabaseName;
            _tableName = $"TestTable_{Guid.NewGuid():N}";
            _tools = fixture.Tools;
        }

        public void Dispose()
        {
            // Per-test cleanup: drop any tables created by this specific test
            // Database cleanup is handled by DatabaseFixture
            _tools.DropTable($"DROP TABLE IF EXISTS {_tableName}", _testDatabaseName).GetAwaiter().GetResult();
        }

        [Fact]
        public async Task CreateTable_ReturnsSuccess_WhenSqlIsValid()
        {
            var sql = $"CREATE TABLE {_tableName} (Id INT PRIMARY KEY)";
            var result = await _tools.CreateTable(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DescribeTable_ReturnsSchema_WhenTableExists()
        {
            // Ensure table exists
            var createResult = await _tools.CreateTable($"CREATE TABLE {_tableName} (Id INT PRIMARY KEY)", _testDatabaseName) as DbOperationResult;
            Assert.NotNull(createResult);
            Assert.True(createResult.Success);

            var result = await _tools.DescribeTable(_tableName, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.True(result.Success);
            var dict = result.Data as System.Collections.IDictionary;
            Assert.NotNull(dict);
            Assert.True(dict.Contains("table"));
            Assert.True(dict.Contains("columns"));
            Assert.True(dict.Contains("indexes"));
            Assert.True(dict.Contains("constraints"));
            var table = dict["table"];
            Assert.NotNull(table);
            var tableType = table.GetType();
            Assert.NotNull(tableType.GetProperty("name"));
            Assert.NotNull(tableType.GetProperty("schema"));
            var columns = dict["columns"] as System.Collections.IEnumerable;
            Assert.NotNull(columns);
        }

        [Fact]
        public async Task DropTable_ReturnsSuccess_WhenSqlIsValid()
        {
            // Create table first
            await _tools.CreateTable($"CREATE TABLE {_tableName} (Id INT PRIMARY KEY)", _testDatabaseName);

            var sql = $"DROP TABLE IF EXISTS {_tableName}";
            var result = await _tools.DropTable(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task InsertData_ReturnsSuccess_WhenSqlIsValid()
        {
            // Ensure table exists
            var createResult = await _tools.CreateTable($"CREATE TABLE {_tableName} (Id INT PRIMARY KEY)", _testDatabaseName) as DbOperationResult;
            Assert.NotNull(createResult);
            Assert.True(createResult.Success);

            var sql = $"INSERT INTO {_tableName} (Id) VALUES (1)";
            var result = await _tools.InsertData(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.RowsAffected.HasValue && result.RowsAffected.Value > 0);
        }

        [Fact]
        public async Task ListTables_ReturnsTables()
        {
            var result = await _tools.ListTables(_testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task ReadData_ReturnsData_WhenSqlIsValid()
        {
            // Ensure table exists and has data
            var createResult = await _tools.CreateTable($"CREATE TABLE {_tableName} (Id INT PRIMARY KEY)", _testDatabaseName) as DbOperationResult;
            Assert.NotNull(createResult);
            Assert.True(createResult.Success);
            var insertResult = await _tools.InsertData($"INSERT INTO {_tableName} (Id) VALUES (1)", _testDatabaseName) as DbOperationResult;
            Assert.NotNull(insertResult);
            Assert.True(insertResult.Success);

            var sql = $"SELECT * FROM {_tableName}";
            var result = await _tools.ReadData(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task UpdateData_ReturnsSuccess_WhenSqlIsValid()
        {
            // Ensure table exists and has data
            var createResult = await _tools.CreateTable($"CREATE TABLE {_tableName} (Id INT PRIMARY KEY)", _testDatabaseName) as DbOperationResult;
            Assert.NotNull(createResult);
            Assert.True(createResult.Success);
            var insertResult = await _tools.InsertData($"INSERT INTO {_tableName} (Id) VALUES (1)", _testDatabaseName) as DbOperationResult;
            Assert.NotNull(insertResult);
            Assert.True(insertResult.Success);

            var sql = $"UPDATE {_tableName} SET Id = 2 WHERE Id = 1";
            var result = await _tools.UpdateData(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.RowsAffected.HasValue);
        }

        [Fact]
        public async Task CreateTable_ReturnsError_WhenSqlIsInvalid()
        {
            var sql = "CREATE TABLE";
            var result = await _tools.CreateTable(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("syntax", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DescribeTable_ReturnsError_WhenTableDoesNotExist()
        {
            var result = await _tools.DescribeTable("NonExistentTable", _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Table 'NonExistentTable' not found.", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DropTable_ReturnsError_WhenSqlIsInvalid()
        {
            var sql = "DROP";
            var result = await _tools.DropTable(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("syntax", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task InsertData_ReturnsError_WhenSqlIsInvalid()
        {
            var sql = "INSERT INTO TestTable";
            var result = await _tools.InsertData(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("syntax", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ReadData_ReturnsError_WhenSqlIsInvalid()
        {
            var sql = "SELECT FROM";
            var result = await _tools.ReadData(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("syntax", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateData_ReturnsError_WhenSqlIsInvalid()
        {
            var sql = "UPDATE TestTable";
            var result = await _tools.UpdateData(sql, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("syntax", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SqlInjection_NotExecuted_When_QueryFails()
        {
            // Ensure table exists
            var createResult = await _tools.CreateTable($"CREATE TABLE {_tableName} (Id INT PRIMARY KEY, Name NVARCHAR(100))", _testDatabaseName) as DbOperationResult;
            Assert.NotNull(createResult);
            Assert.True(createResult.Success);

            // Attempt SQL Injection
            var maliciousInput = "1; DROP TABLE " + _tableName + "; --";
            var sql = $"INSERT INTO {_tableName} (Id, Name) VALUES ({maliciousInput}, 'Malicious')";
            var result = await _tools.InsertData(sql, _testDatabaseName) as DbOperationResult;

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("syntax", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            // Verify table still exists
            var describeResult = await _tools.DescribeTable(_tableName, _testDatabaseName) as DbOperationResult;
            Assert.NotNull(describeResult);
            Assert.True(describeResult.Success);
        }

        [Fact]
        public async Task CreateDatabase_ReturnsSuccess_WhenSqlIsValid()
        {
            var tempDbName = $"TempDB_{Guid.NewGuid():N}";
            var sql = $"CREATE DATABASE [{tempDbName}]";
            var result = await _tools.CreateDatabase(sql) as DbOperationResult;
            Assert.NotNull(result);
            Assert.True(result.Success);

            // Cleanup
            await _tools.DropDatabase($"DROP DATABASE [{tempDbName}]");
        }

        [Fact]
        public async Task CreateDatabase_ReturnsError_WhenDatabaseAlreadyExists()
        {
            var tempDbName = $"TempDB_{Guid.NewGuid():N}";
            
            // Create database first
            var createSql = $"CREATE DATABASE [{tempDbName}]";
            var createResult = await _tools.CreateDatabase(createSql) as DbOperationResult;
            Assert.NotNull(createResult);
            Assert.True(createResult.Success);

            // Try to create the same database again
            var result = await _tools.CreateDatabase(createSql) as DbOperationResult;
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("already exists", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            // Cleanup
            await _tools.DropDatabase($"DROP DATABASE [{tempDbName}]");
        }

        [Fact]
        public async Task CreateDatabase_ReturnsError_WhenSqlIsInvalid()
        {
            var sql = "CREATE DATABASE";
            var result = await _tools.CreateDatabase(sql) as DbOperationResult;
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("syntax", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DropDatabase_ReturnsSuccess_WhenSqlIsValid()
        {
            var tempDbName = $"TempDB_{Guid.NewGuid():N}";
            
            // Create database first
            var createSql = $"CREATE DATABASE [{tempDbName}]";
            var createResult = await _tools.CreateDatabase(createSql) as DbOperationResult;
            Assert.NotNull(createResult);
            Assert.True(createResult.Success);

            // Drop the database
            var dropSql = $"DROP DATABASE [{tempDbName}]";
            var result = await _tools.DropDatabase(dropSql) as DbOperationResult;
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DropDatabase_ReturnsError_WhenSqlIsInvalid()
        {
            var sql = "DROP DATABASE";
            var result = await _tools.DropDatabase(sql) as DbOperationResult;
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("syntax", result.Error ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ListDatabases_ReturnsSuccess()
        {
            var result = await _tools.ListDatabases() as DbOperationResult;
            Assert.NotNull(result);
            
            if (!result.Success)
            {
                // Log the error for debugging
                throw new Exception($"ListDatabases failed: {result.Error}");
            }
            
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            
            var databases = result.Data as System.Collections.IEnumerable;
            Assert.NotNull(databases);
            
            // Should have at least our test database
            var dbList = databases.Cast<object>().ToList();
            Assert.NotEmpty(dbList);
        }
    }
}