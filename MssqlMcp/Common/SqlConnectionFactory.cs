using Microsoft.Data.SqlClient;

namespace Mssql.McpServer.Common;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    public async Task<SqlConnection> GetOpenConnectionAsync(string databaseName)
    {
        return await GetOpenConnectionAsync(databaseName, null, null);
    }

    public async Task<SqlConnection> GetOpenConnectionAsync(string databaseName, string? server, bool? useWindowsAuth = null)
    {
        var connectionString = GetConnectionString();

        var sqb = new SqlConnectionStringBuilder(connectionString);
        
        // Override database
        sqb.InitialCatalog = databaseName;
        
        // Override server if provided
        if (!string.IsNullOrWhiteSpace(server))
        {
            sqb.DataSource = server;
        }
        
        // Override authentication if specified
        if (useWindowsAuth.HasValue)
        {
            if (useWindowsAuth.Value)
            {
                sqb.IntegratedSecurity = true;
                sqb.Authentication = SqlAuthenticationMethod.NotSpecified;
            }
            else
            {
                sqb.IntegratedSecurity = false;
            }
        }

        var conn = new SqlConnection(sqb.ToString());
        await conn.OpenAsync();
        return conn;
    }

    private static string GetConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

        return string.IsNullOrEmpty(connectionString)
            ? throw new InvalidOperationException("Connection string is not set in the environment variable 'CONNECTION_STRING'.\n\nHINT: Have a local SQL Server, with a database called 'test', from console, run `SET CONNECTION_STRING=Server=.;Database=test;Trusted_Connection=True;TrustServerCertificate=True` and the load the .sln file")
            : connectionString;
    }
}
