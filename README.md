# Mssql SQL MCP Server (.NET 8)

This project is a .NET 8 console application implementing a Model Context Protocol (MCP) server for MSSQL Databases using the official [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk).

## Features

- Provide connection string via environment variable `CONNECTION_STRING`.
- **Dynamic Server Connection**: All tools accept an optional `server` parameter to connect to different SQL Server instances on-the-fly.
- **MCP Tools Implemented**:
  - **Database Operations**:
    - CreateDatabase: Create new databases.
    - DropDatabase: Drop existing databases.
    - ListDatabases: List all user databases on the SQL Server instance.
  - **Table Operations**:
    - ListTables: List all tables in a database.
    - DescribeTable: Get schema/details for a table.
    - CreateTable: Create new tables.
    - DropTable: Drop existing tables.
  - **Data Operations**:
    - InsertData: Insert data into tables.
    - ReadData: Read/query data from tables.
    - UpdateData: Update values in tables.
- **Logging**: Console logging using Microsoft.Extensions.Logging.
- **Unit Tests**: xUnit-based unit tests for all major components.

## Getting Started

### Prerequisites

- Access to a SQL Server or Azure SQL Database

### Setup

1. **Build **

---
```sh
   cd MssqlMcp
   dotnet build
```
---


2a. **Visual Studio: Add MCP Server config to Visual Studio Settings**

In Visual Studio, go to Tools > Options > GitHub Copilot > Model Context Protocol.

Click "Add" to add a new MCP Server with the following settings:

- **Name**: MSSQL MCP
- **Type**: stdio
- **Command**: `C:\src\MssqlMcp\MssqlMcp\bin\Debug\net8.0\MssqlMcp.exe`
- **Environment Variables**:
  - Key: `CONNECTION_STRING`
  - Value: `Server=.;Trusted_Connection=True;TrustServerCertificate=True`

NOTE: Replace the path with the actual location of your repo on your machine. 

**Important**: The connection string should NOT include a database name. Each tool operation will specify which database to use.

**Run the MCP Server**

Click "OK" to save the settings. The MCP Server should now be available in Visual Studio.

Open GitHub Copilot Chat (Ctrl+Shift+I or View > GitHub Copilot Chat), make sure Agent Mode is enabled.

Click the tools icon, and ensure the "MSSQL MCP" tools are selected.

Then type in the chat window "List tables in database 'test'" and hit enter. (If you have other tools loaded, you may need to specify "MSSQL MCP" in the initial prompt, e.g. "Using MSSQL MCP, list tables in database 'test'").

2b. **VSCode: Start VSCode, and add MCP Server config to VSCode Settings**

Load the settings file in VSCode (Ctrl+Shift+P > Preferences: Open Settings (JSON)).

Add a new MCP Server with the following settings:

---
```json
    "MSSQL MCP": {
        "type": "stdio",
        "command": "C:\\src\\MssqlMcp\\MssqlMcp\\bin\\Debug\\net8.0\\MssqlMcp.exe",
        "env": {
            "CONNECTION_STRING": "Server=.;Trusted_Connection=True;TrustServerCertificate=True"
            }
}
```
---

NOTE: Replace the path with the location of your repo on your machine.

**Important**: The connection string should NOT include a database name (`Initial Catalog` or `Database`). Each tool operation will specify which database to use.

e.g. your MCP settings should look like this if "MSSQL MCP" is your own MCP Server in VSCode settings:

---
```json
"mcp": {
    "servers": {
        "MSSQL MCP": {
            "type": "stdio",
            "command": "C:\\src\\MssqlMcp\\MssqlMcp\\bin\\Debug\\net8.0\\MssqlMcp.exe",
                "env": {
                "CONNECTION_STRING": "Server=.;Trusted_Connection=True;TrustServerCertificate=True"
            }
    }
}
```
---

**Run the MCP Server**

Save the Settings file, and then you should see the "Start" button appear in the settings.json.  Click "Start" to start the MCP Server. (You can then click on "Running" to view the Output window).

Start Chat (Ctrl+Shift+I), make sure Agent Mode is selected.

Click the tools icon, and ensure the "MSSQL MCP" tools are selected.

Then type in the chat window "List tables in database 'test'" and hit enter. (If you have other tools loaded, you may need to specify "MSSQL MCP" in the initial prompt, e.g. "Using MSSQL MCP, list tables in database 'test'").

2c. **Claude Desktop: Add MCP Server config to Claude Desktop**

Press File > Settings > Developer.
Press the "Edit Config" button (which will load the claude_desktop_config.json file in your editor).

Add a new MCP Server with the following settings:

---
```json
{
    "mcpServers": {
        "MSSQL MCP": {
            "command": "C:\\src\\MssqlMcp\\MssqlMcp\\bin\\Debug\\net8.0\\MssqlMcp.exe",
            "env": {
                    "CONNECTION_STRING": "Server=.;Trusted_Connection=True;TrustServerCertificate=True"
                }
        }
    }
}
```
---

**Important**: The connection string should NOT include a database name. Each tool operation will specify which database to use.

Save the file, start a new Chat, you'll see the "Tools" icon, it should list 10 MSSQL MCP tools.

## Usage Notes

### Dynamic Server Connection
All tools accept an optional `server` parameter that allows you to connect to different SQL Server instances without changing the MCP server configuration:

- **Default behavior**: If `server` is not provided, the tool uses the server from the `CONNECTION_STRING` environment variable.
- **Override server**: Pass a `server` parameter to connect to a different SQL Server instance.

Example prompts:
- "List databases" (uses default server)
- "List databases on server 'myserver.database.windows.net'" (connects to Azure SQL)
- "List tables in database 'mydb' on server 'localhost\\SQLEXPRESS'" (connects to SQL Express)

### Database Operations
- **CreateDatabase**: Creates a new database on the SQL Server instance.
- **DropDatabase**: Drops an existing database. Use with caution as this is a destructive operation.
- **ListDatabases**: Lists all user databases on the SQL Server instance (excludes system databases: master, tempdb, model, msdb). Returns database name, ID, creation date, state, recovery model, and size in MB.

### Table and Data Operations
- **All table and data operations require a database name parameter**. This allows you to work with multiple databases from a single MCP server instance.
- Example prompts:
  - "List all databases"
  - "List tables in database 'test'"
  - "Create a table named 'users' in database 'myapp'"
  - "Read data from table 'customers' in database 'sales'"
  - "List tables in database 'test' on server 'myserver.database.windows.net'"

### Connection String
- The connection string provided in the environment variable is used as the default server connection.
- **Do NOT include a database name** (`Initial Catalog` or `Database`) in the connection string.
- Each tool operation specifies which database to use via a parameter.
- You can override the server at runtime using the optional `server` parameter.
- For database creation/deletion, the connection uses the master database.

# Troubleshooting

1. If you get a "Task canceled" error using "Active Directory Default", try "Active Directory Interactive".
2. Ensure your SQL Server user has appropriate permissions for database creation if using CreateDatabase.
3. When working with multiple databases, always specify the database name in your prompts to avoid confusion.
4. Make sure your connection string does NOT include a database name - the MCP tools will handle database selection dynamically.
5. When connecting to remote servers, ensure your firewall rules and authentication settings are properly configured.



