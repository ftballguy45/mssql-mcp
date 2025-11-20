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
        [Description("Database name to execute query against")] string databaseName)
    {
        var conn = await _connectionFactory.GetOpenConnectionAsync(databaseName);
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
