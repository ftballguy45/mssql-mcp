using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Mssql.McpServer.Common;

namespace Mssql.McpServer;

public partial class Tools
{
    [McpServerTool(
        Title = "Drop Database",
        ReadOnly = false,
        Destructive = true),
        Description("Drops a database from the SQL Server. Expects a valid DROP DATABASE SQL statement as input.")]
    public async Task<DbOperationResult> DropDatabase(
        [Description("DROP DATABASE SQL statement")] string sql,
        [Description("Optional: SQL Server name/address to connect to (e.g., '.', 'localhost', 'server.database.windows.net'). If not provided, uses the default from CONNECTION_STRING.")] string? server = null)
    {
        // Connect to master database to drop a database
        var conn = await _connectionFactory.GetOpenConnectionAsync("master", server);
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
            _logger.LogError(ex, "DropDatabase failed: {Message}", ex.Message);
            return new DbOperationResult(success: false, error: ex.Message);
        }
    }
}
