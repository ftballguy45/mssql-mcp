using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Mssql.McpServer.Common;

namespace Mssql.McpServer;

public partial class Tools
{
    private const string ListDatabasesQuery = @"
        SELECT 
            name,
            database_id,
            create_date,
            state_desc,
            recovery_model_desc,
            (SELECT SUM(size) * 8 / 1024 FROM sys.master_files WHERE database_id = d.database_id) AS size_mb
        FROM sys.databases d
        WHERE database_id > 4
        ORDER BY name";

    [McpServerTool(
        Title = "List Databases",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false),
        Description("Lists all user databases on the SQL Server instance (excludes system databases).")]
    public async Task<DbOperationResult> ListDatabases(
        [Description("Optional: SQL Server name/address to connect to (e.g., '.', 'localhost', 'server.database.windows.net'). If not provided, uses the default from CONNECTION_STRING.")] string? server = null)
    {
        var conn = await _connectionFactory.GetOpenConnectionAsync("master", server);
        try
        {
            using (conn)
            {
                using var cmd = new SqlCommand(ListDatabasesQuery, conn);
                var databases = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    databases.Add(new
                    {
                        name = reader.GetString(0),
                        database_id = reader.GetInt32(1),
                        create_date = reader.GetDateTime(2),
                        state = reader.GetString(3),
                        recovery_model = reader.GetString(4),
                        size_mb = reader.IsDBNull(5) ? 0 : Convert.ToInt64(reader.GetValue(5))
                    });
                }
                return new DbOperationResult(success: true, data: databases);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ListDatabases failed: {Message}", ex.Message);
            return new DbOperationResult(success: false, error: ex.Message);
        }
    }
}
