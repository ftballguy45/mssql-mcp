-- Script to clean up orphaned test databases
-- Run this if test databases are left behind from failed test runs

USE master;
GO

DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql = @sql + 'DROP DATABASE [' + name + '];' + CHAR(13) + CHAR(10)
FROM sys.databases
WHERE name LIKE 'TestDB_%' OR name LIKE 'TempDB_%';

PRINT @sql;  -- Preview what will be dropped
-- EXEC sp_executesql @sql;  -- Uncomment to actually execute
