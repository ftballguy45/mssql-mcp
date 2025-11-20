using Microsoft.Data.SqlClient;

namespace Mssql.McpServer.Common;

/// <summary>
/// Defines a factory interface for creating SQL database connections.
/// </summary>
public interface ISqlConnectionFactory
{
    Task<SqlConnection> GetOpenConnectionAsync(string databaseName);
}