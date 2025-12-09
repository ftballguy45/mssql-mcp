using System.ComponentModel;
using ModelContextProtocol.Server;
using Mssql.McpServer.Common;

namespace Mssql.McpServer;

public partial class Tools
{
    [McpServerTool(
        Title = "Get Connection Help",
        ReadOnly = true,
        Idempotent = true,
        Destructive = false),
        Description("Returns information about how to connect to different SQL Server types. Use this to understand what connection parameters are needed for a specific scenario.")]
    public Task<DbOperationResult> GetConnectionHelp(
        [Description("The type of SQL Server to connect to. Options: 'local' (localhost/.), 'named-instance' (localhost\\SQLEXPRESS), 'remote' (on-premises server), 'azure-sql' (Azure SQL Database), 'azure-sql-mi' (Azure SQL Managed Instance)")] string serverType = "local")
    {
        object help = serverType.ToLowerInvariant() switch
        {
            "local" => new
            {
                description = "Local SQL Server (default instance)",
                requiredParameters = new[] { "databaseName" },
                optionalParameters = new[] { "server (defaults to '.')" },
                exampleServer = ".",
                authenticationOptions = new[]
                {
                    "Windows Authentication (default) - no userId/password needed",
                    "SQL Authentication - provide userId and password"
                },
                examplePrompt = "List tables in database 'mydb'"
            },
            "named-instance" => new
            {
                description = "SQL Server named instance (e.g., SQL Express)",
                requiredParameters = new[] { "databaseName", "server (include instance name)" },
                optionalParameters = Array.Empty<string>(),
                exampleServer = "localhost\\SQLEXPRESS or .\\SQLEXPRESS",
                authenticationOptions = new[]
                {
                    "Windows Authentication (default) - no userId/password needed",
                    "SQL Authentication - provide userId and password"
                },
                examplePrompt = "List tables in database 'mydb' on server '.\\SQLEXPRESS'"
            },
            "remote" => new
            {
                description = "Remote on-premises SQL Server",
                requiredParameters = new[] { "databaseName", "server" },
                optionalParameters = new[] { "userId", "password", "encrypt", "trustServerCertificate" },
                exampleServer = "sqlserver.mycompany.com or 192.168.1.100",
                authenticationOptions = new[]
                {
                    "Windows Authentication - if on same domain, no userId/password needed",
                    "SQL Authentication - provide userId and password"
                },
                examplePrompt = "List tables in database 'mydb' on server 'sqlserver.mycompany.com' with userId 'sa' and password 'MyPassword123'"
            },
            "azure-sql" => new
            {
                description = "Azure SQL Database",
                requiredParameters = new[] { "databaseName", "server", "userId", "password" },
                optionalParameters = new[] { "encrypt (defaults to true for Azure)" },
                exampleServer = "myserver.database.windows.net",
                authenticationOptions = new[]
                {
                    "SQL Authentication - provide userId and password",
                    "Azure AD Authentication - use 'Authentication=Active Directory Interactive' in CONNECTION_STRING env var"
                },
                notes = new[]
                {
                    "Server format: <servername>.database.windows.net",
                    "Encryption is mandatory for Azure SQL",
                    "Ensure your IP is allowed in Azure SQL firewall rules"
                },
                examplePrompt = "List tables in database 'mydb' on server 'myserver.database.windows.net' with userId 'myadmin' and password 'MyPassword123'"
            },
            "azure-sql-mi" => new
            {
                description = "Azure SQL Managed Instance",
                requiredParameters = new[] { "databaseName", "server", "userId", "password" },
                optionalParameters = new[] { "encrypt (defaults to true)" },
                exampleServer = "myinstance.abc123.database.windows.net",
                authenticationOptions = new[]
                {
                    "SQL Authentication - provide userId and password",
                    "Azure AD Authentication - configure in CONNECTION_STRING env var"
                },
                notes = new[]
                {
                    "Server format: <instancename>.<dns-zone>.database.windows.net",
                    "Must be accessed from within VNet or via public endpoint"
                },
                examplePrompt = "List tables in database 'mydb' on server 'myinstance.abc123.database.windows.net' with userId 'myadmin' and password 'MyPassword123'"
            },
            _ => new
            {
                description = "Unknown server type. Use 'local', 'named-instance', 'remote', 'azure-sql', or 'azure-sql-mi'",
                requiredParameters = Array.Empty<string>(),
                optionalParameters = Array.Empty<string>(),
                exampleServer = "",
                authenticationOptions = Array.Empty<string>(),
                examplePrompt = "Get connection help for 'azure-sql'"
            }
        };

        return Task.FromResult(new DbOperationResult(success: true, data: help));
    }
}
