using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Mssql.McpServer.Common;

namespace Mssql.McpServer;

public partial class Tools
{
    [McpServerTool(
        Title = "Read Data",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false),
        Description("Executes SQL queries against SQL Database to read data")]
    public async Task<DbOperationResult> ReadData(
        [Description("SQL query to execute")] string sql,
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
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                var results = new List<Dictionary<string, object?>>();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object?>();
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    results.Add(row);
                }
                return new DbOperationResult(success: true, data: results);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReadData failed: {Message}", ex.Message);
            return new DbOperationResult(success: false, error: ex.Message);
        }
    }
}
