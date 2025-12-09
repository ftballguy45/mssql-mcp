using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Mssql.McpServer.Common;

namespace Mssql.McpServer;

public partial class Tools
{
    [McpServerTool(
        Title = "Create Database",
        ReadOnly = false,
        Destructive = false),
        Description("Creates a new database on the SQL Server. Expects a valid CREATE DATABASE SQL statement as input.")]
    public async Task<DbOperationResult> CreateDatabase(
        [Description("CREATE DATABASE SQL statement")] string sql,
        [Description("Optional: SQL Server name/address (e.g., '.', 'localhost', 'server.database.windows.net'). If not provided, uses the default from CONNECTION_STRING.")] string? server = null,
        [Description("Optional: SQL Server user ID for SQL authentication.")] string? userId = null,
        [Description("Optional: SQL Server password for SQL authentication.")] string? password = null,
        [Description("Optional: Set to true to trust the server certificate.")] bool? trustServerCertificate = null,
        [Description("Optional: Set to true to encrypt the connection.")] bool? encrypt = null)
    {
        // Connect to master database to create a new database
        var conn = await _connectionFactory.GetOpenConnectionAsync("master", server, userId, password, null, trustServerCertificate, encrypt);
        try
        {
            using (conn)
            {
                using var cmd = new SqlCommand(sql, conn);
                _ = await cmd.ExecuteNonQueryAsync();
                return new DbOperationResult(success: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateDatabase failed: {Message}", ex.Message);
            return new DbOperationResult(success: false, error: ex.Message);
        }
    }
}
