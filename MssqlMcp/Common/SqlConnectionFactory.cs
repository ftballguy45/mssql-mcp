using Microsoft.Data.SqlClient;

namespace Mssql.McpServer.Common;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    public async Task<SqlConnection> GetOpenConnectionAsync(string databaseName)
    {
        var connectionString = GetConnectionString();

        var sqb = new SqlConnectionStringBuilder(connectionString);
        sqb.InitialCatalog = databaseName;

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
