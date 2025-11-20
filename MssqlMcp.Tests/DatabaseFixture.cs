using Microsoft.Extensions.Logging;
using Moq;
using Mssql.McpServer;
using Mssql.McpServer.Common;

namespace MssqlMcp.Tests;

/// <summary>
/// Shared fixture for all tests - creates one database for the entire test run
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    public string TestDatabaseName { get; private set; } = string.Empty;
    public Tools Tools { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Create a single test database for all tests
        TestDatabaseName = $"TestDB_{Guid.NewGuid():N}";
        
        var connectionFactory = new SqlConnectionFactory();
        var loggerMock = new Mock<ILogger<Tools>>();
        Tools = new Tools(connectionFactory, loggerMock.Object);

        // Create the test database
        var createDbSql = $"CREATE DATABASE [{TestDatabaseName}]";
        var result = await Tools.CreateDatabase(createDbSql) as DbOperationResult;
        if (result == null || !result.Success)
        {
            throw new InvalidOperationException($"Failed to create test database: {result?.Error}");
        }
    }

    public async Task DisposeAsync()
    {
        // Drop the test database after all tests complete
        if (!string.IsNullOrEmpty(TestDatabaseName))
        {
            try
            {
                // First, kill all connections to the test database
                var killConnectionsSql = $@"
                    IF EXISTS (SELECT * FROM sys.databases WHERE name = '{TestDatabaseName}')
                    BEGIN
                        ALTER DATABASE [{TestDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        DROP DATABASE [{TestDatabaseName}];
                    END";
                
                await Tools.DropDatabase(killConnectionsSql);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - we're in cleanup
                Console.WriteLine($"Warning: Failed to drop test database {TestDatabaseName}: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// Collection definition - all tests using this collection share the same DatabaseFixture instance
/// </summary>
[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
