using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Mssql.McpServer.Common;

namespace Mssql.McpServer;

public partial class Tools
{
    [McpServerTool(
        Title = "Drop Table",
        ReadOnly = false,
        Destructive = true),
        Description("Drops a table in the SQL Database. Expects a valid DROP TABLE SQL statement as input.")]
    public async Task<DbOperationResult> DropTable(
        [Description("DROP TABLE SQL statement")] string sql,
        [Description("Database name to execute query against")] string databaseName,
        [Description("Optional: SQL Server name/address to connect to (e.g., '.', 'localhost', 'server.database.windows.net'). If not provided, uses the default from CONNECTION_STRING.")] string? server = null)
    {
        var conn = await _connectionFactory.GetOpenConnectionAsync(databaseName, server);
        try
        {
            using (conn)
            {
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
                _ = await cmd.ExecuteNonQueryAsync();
                return new DbOperationResult(success: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DropTable failed: {Message}", ex.Message);
            return new DbOperationResult(success: false, error: ex.Message);
        }
    }
}
