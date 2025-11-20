using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Mssql.McpServer.Common;

namespace Mssql.McpServer;

public partial class Tools
{
    [McpServerTool(
        Title = "Create Table",
        ReadOnly = false,
        Destructive = false),
        Description("Creates a new table in the SQL Database. Expects a valid CREATE TABLE SQL statement as input.")]
    public async Task<DbOperationResult> CreateTable(
        [Description("CREATE TABLE SQL statement")] string sql,
        [Description("Database name to execute query against")] string databaseName)
    {
        var conn = await _connectionFactory.GetOpenConnectionAsync(databaseName);
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
            _logger.LogError(ex, "CreateTable failed: {Message}", ex.Message);
            return new DbOperationResult(success: false, error: ex.Message);
        }
    }
}
