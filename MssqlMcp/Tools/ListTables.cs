using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Mssql.McpServer.Common;

namespace Mssql.McpServer;

public partial class Tools
{
    private const string ListTablesQuery = @"SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_SCHEMA, TABLE_NAME";

    [McpServerTool(
        Title = "List Tables",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false),
        Description("Lists all tables in the SQL Database.")]
    public async Task<DbOperationResult> ListTables(
        [Description("Database name to execute query against")] string databaseName,
        [Description("Optional: SQL Server name/address (e.g., '.', 'localhost', 'server.database.windows.net'). If not provided, uses the default from CONNECTION_STRING.")] string? server = null,
        [Description("Optional: SQL Server user ID for SQL authentication.")] string? userId = null,
        [Description("Optional: SQL Server password for SQL authentication.")] string? password = null,
        [Description("Optional: Set to true to trust the server certificate.")] bool? trustServerCertificate = null,
        [Description("Optional: Set to true to encrypt the connection.")] bool? encrypt = null)
    {
        var conn = await _connectionFactory.GetOpenConnectionAsync(databaseName, server, userId, password, null, trustServerCertificate, encrypt);
        try
        {
            using (conn)
            {
                using var cmd = new SqlCommand(ListTablesQuery, conn);
                var tables = new List<string>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add($"{reader.GetString(0)}.{reader.GetString(1)}");
                }
                return new DbOperationResult(success: true, data: tables);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ListTables failed: {Message}", ex.Message);
            return new DbOperationResult(success: false, error: ex.Message);
        }
    }
}
