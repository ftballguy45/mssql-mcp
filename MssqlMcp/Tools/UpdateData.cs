using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Mssql.McpServer.Common;

namespace Mssql.McpServer;

public partial class Tools
{
    [McpServerTool(
        Title = "Update Data",
        ReadOnly = false,
        Destructive = true),
        Description("Updates data in a table in the SQL Database. Expects a valid UPDATE SQL statement as input.")]
    public async Task<DbOperationResult> UpdateData(
        [Description("UPDATE SQL statement")] string sql,
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
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
                var rows = await cmd.ExecuteNonQueryAsync();
                return new DbOperationResult(true, null, rows);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateData failed: {Message}", ex.Message);
            return new DbOperationResult(false, ex.Message);
        }
    }
}

