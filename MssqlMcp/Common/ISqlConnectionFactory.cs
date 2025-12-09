using Microsoft.Data.SqlClient;

namespace Mssql.McpServer.Common;

/// <summary>
/// Defines a factory interface for creating SQL database connections.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Gets an open connection to the specified database using the environment connection string.
    /// </summary>
    Task<SqlConnection> GetOpenConnectionAsync(string databaseName);

    /// <summary>
    /// Gets an open connection to the specified database with optional server override.
    /// </summary>
    /// <param name="databaseName">The database name to connect to.</param>
    /// <param name="server">Optional server name (e.g., ".", "localhost", "server.database.windows.net").</param>
    /// <param name="useWindowsAuth">If true, uses Windows/Integrated authentication. If false, uses the connection string's auth method.</param>
    Task<SqlConnection> GetOpenConnectionAsync(string databaseName, string? server, bool? useWindowsAuth = null);
}