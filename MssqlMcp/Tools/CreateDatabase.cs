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
        [Description("CREATE DATABASE SQL statement")] string sql)
    {
        // Connect to master database to create a new database
        var conn = await _connectionFactory.GetOpenConnectionAsync("master");
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
