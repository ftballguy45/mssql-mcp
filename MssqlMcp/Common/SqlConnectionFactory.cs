using Microsoft.Data.SqlClient;

namespace Mssql.McpServer.Common;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    public async Task<SqlConnection> GetOpenConnectionAsync(string databaseName)
    {
        return await GetOpenConnectionAsync(databaseName, null, null, null, null, null, null);
    }

    public async Task<SqlConnection> GetOpenConnectionAsync(
        string databaseName, 
        string? server = null, 
        string? userId = null, 
        string? password = null,
        bool? useWindowsAuth = null,
        bool? trustServerCertificate = null,
        bool? encrypt = null)
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
        
        // Handle authentication
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(password))
        {
            // SQL Server authentication
            sqb.UserID = userId;
            sqb.Password = password;
            sqb.IntegratedSecurity = false;
            sqb.Authentication = SqlAuthenticationMethod.SqlPassword;
        }
        else if (useWindowsAuth == true)
        {
            // Windows/Integrated authentication
            sqb.IntegratedSecurity = true;
            sqb.Authentication = SqlAuthenticationMethod.NotSpecified;
        }
        
        // Override trust server certificate if specified
        if (trustServerCertificate.HasValue)
        {
            sqb.TrustServerCertificate = trustServerCertificate.Value;
        }
        
        // Override encryption if specified
        if (encrypt.HasValue)
        {
            sqb.Encrypt = encrypt.Value ? SqlConnectionEncryptOption.Mandatory : SqlConnectionEncryptOption.Optional;
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
