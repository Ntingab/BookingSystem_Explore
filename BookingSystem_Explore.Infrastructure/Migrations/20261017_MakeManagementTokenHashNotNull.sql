-- Migration: Make ManagementTokenHash NOT NULL on Appointments
-- Run this script against the target database (SQL Server).

BEGIN TRANSACTION;

-- Ensure no NULL values remain
UPDATE dbo.Appointments
SET ManagementTokenHash = ''
WHERE ManagementTokenHash IS NULL;

-- Alter column to be NOT NULL (max length 256 assumed)
ALTER TABLE dbo.Appointments
ALTER COLUMN ManagementTokenHash NVARCHAR(256) NOT NULL;

COMMIT TRANSACTION;

-- Note: If you use EF Core migrations, you can create a proper Migration class instead and run `dotnet ef migrations add MakeManagementTokenHashNotNull` then `dotnet ef database update`.
