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
    /// Gets an open connection to the specified database with optional connection overrides.
    /// </summary>
    /// <param name="databaseName">The database name to connect to.</param>
    /// <param name="server">Optional server name (e.g., ".", "localhost", "server.database.windows.net").</param>
    /// <param name="userId">Optional SQL authentication user ID. If provided with password, uses SQL auth instead of Windows auth.</param>
    /// <param name="password">Optional SQL authentication password.</param>
    /// <param name="useWindowsAuth">Optional: If true, uses Windows/Integrated authentication. If false with userId/password, uses SQL auth.</param>
    /// <param name="trustServerCertificate">Optional: Trust the server certificate (useful for dev/test environments).</param>
    /// <param name="encrypt">Optional: Encrypt the connection (required for Azure SQL).</param>
    Task<SqlConnection> GetOpenConnectionAsync(
        string databaseName, 
        string? server = null, 
        string? userId = null, 
        string? password = null,
        bool? useWindowsAuth = null,
        bool? trustServerCertificate = null,
        bool? encrypt = null);
}