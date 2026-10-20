
/*
    BookingSystem_Explore
    SQL Server Database Schema

    Tables:
      1. Branches
      2. Services
      3. BranchServices
      4. AppointmentSlots
      5. Customers
      6. Appointments
      7. Notifications

    IMPROVEMENTS:
    - Preserves all seven original tables, columns and relationships.
    - Explicit reset switch prevents accidental deletion of existing data.
    - Optional demo data (off by default).
    - Validates positive service duration and valid slot dates.
    - Adds targeted indexes for booking searches and notification processing.

    IMPORTANT: This is a full-schema creation/reset script, not a migration.
    Existing data is NEVER dropped unless @AllowDestructiveReset = 1.
*/

USE [master];
GO

BEGIN TRY
    IF NOT EXISTS (
        SELECT 1
        FROM sys.databases
        WHERE name = 'BookingSystem'
    )
    BEGIN
        CREATE DATABASE [BookingSystem];
        PRINT 'SUCCESS: BookingSystem database created successfully.';
    END
    ELSE
    BEGIN
        PRINT 'INFO: BookingSystem database already exists.';
    END
END TRY
BEGIN CATCH
    PRINT 'ERROR: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO

USE [BookingSystem];
GO

SELECT
    DB_NAME() AS DatabaseName,
    'Database is ready' AS Status;
GO


USE [BookingSystem];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Set to 1 ONLY if you deliberately want to DROP all seven tables/data.
DECLARE @AllowDestructiveReset BIT = 0;
-- Set to 1 for development-only sample records.
DECLARE @IncludeSampleData BIT = 0;

BEGIN TRY
    -- Refuse partial or accidental replacement of an existing schema.
    IF @AllowDestructiveReset = 0 AND
    (
        OBJECT_ID(N'dbo.Branches', N'U') IS NOT NULL OR
        OBJECT_ID(N'dbo.Services', N'U') IS NOT NULL OR
        OBJECT_ID(N'dbo.BranchServices', N'U') IS NOT NULL OR
        OBJECT_ID(N'dbo.AppointmentSlots', N'U') IS NOT NULL OR
        OBJECT_ID(N'dbo.Customers', N'U') IS NOT NULL OR
        OBJECT_ID(N'dbo.Appointments', N'U') IS NOT NULL OR
        OBJECT_ID(N'dbo.Notifications', N'U') IS NOT NULL
    )
        THROW 51000, 'BookingSystem schema already exists. No changes made. Use a migration for an existing database or explicitly enable destructive reset.', 1;

    BEGIN TRANSACTION;

    ------------------------------------------------------
    -- 1. DROP TABLES IN REVERSE DEPENDENCY ORDER (RESET ONLY)
    ------------------------------------------------------

    IF @AllowDestructiveReset = 1
    BEGIN
        IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL
            DROP TABLE dbo.Notifications;

    IF OBJECT_ID('dbo.Appointments', 'U') IS NOT NULL
        DROP TABLE dbo.Appointments;

    IF OBJECT_ID('dbo.AppointmentSlots', 'U') IS NOT NULL
        DROP TABLE dbo.AppointmentSlots;

    IF OBJECT_ID('dbo.BranchServices', 'U') IS NOT NULL
        DROP TABLE dbo.BranchServices;

    IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL
        DROP TABLE dbo.Customers;

    IF OBJECT_ID('dbo.Services', 'U') IS NOT NULL
        DROP TABLE dbo.Services;

        IF OBJECT_ID('dbo.Branches', 'U') IS NOT NULL
            DROP TABLE dbo.Branches;
    END;

    ------------------------------------------------------
    -- 2. BRANCHES
    ------------------------------------------------------

    CREATE TABLE dbo.Branches
    (
        BranchId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_Branches_Id DEFAULT NEWID(),

        BranchName NVARCHAR(200) NOT NULL,
        BranchCode NVARCHAR(50) NOT NULL,
        TimeZoneId NVARCHAR(100) NOT NULL
            CONSTRAINT DF_Branches_TimeZone DEFAULT 'UTC',

        Address NVARCHAR(500) NOT NULL,
        City NVARCHAR(150) NOT NULL,
        Province NVARCHAR(150) NOT NULL,
        ContactNumber NVARCHAR(50) NOT NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_Branches_IsActive DEFAULT 1,

        CreatedDate DATETIME2 NOT NULL
            CONSTRAINT DF_Branches_CreatedDate
            DEFAULT SYSUTCDATETIME(),

        UpdatedDate DATETIME2 NULL,

        CONSTRAINT PK_Branches
            PRIMARY KEY (BranchId),

        CONSTRAINT UQ_Branches_BranchCode
            UNIQUE (BranchCode)
    );

    ------------------------------------------------------
    -- 3. SERVICES
    -- Duration defaults to 30 minutes and must be positive.
    ------------------------------------------------------

    CREATE TABLE dbo.Services
    (
        ServiceId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_Services_Id DEFAULT NEWID(),

        ServiceName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,

        DurationMinutes INT NOT NULL
            CONSTRAINT DF_Services_DurationMinutes DEFAULT (30),

        IsActive BIT NOT NULL
            CONSTRAINT DF_Services_IsActive DEFAULT 1,

        CreatedDate DATETIME2 NOT NULL
            CONSTRAINT DF_Services_CreatedDate
            DEFAULT SYSUTCDATETIME(),

        UpdatedDate DATETIME2 NULL,

        CONSTRAINT PK_Services
            PRIMARY KEY (ServiceId),

        CONSTRAINT CHK_Services_DurationMinutes
            CHECK (DurationMinutes > 0)
    );

    ------------------------------------------------------
    -- 4. BRANCH SERVICES
    ------------------------------------------------------

    CREATE TABLE dbo.BranchServices
    (
        BranchServiceId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_BranchServices_Id DEFAULT NEWID(),

        BranchId UNIQUEIDENTIFIER NOT NULL,
        ServiceId UNIQUEIDENTIFIER NOT NULL,

        Price DECIMAL(18,2) NOT NULL
            CONSTRAINT DF_BranchServices_Price DEFAULT 0,

        CreatedDate DATETIME2 NOT NULL
            CONSTRAINT DF_BranchServices_CreatedDate
            DEFAULT SYSUTCDATETIME(),

        UpdatedDate DATETIME2 NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_BranchServices_IsActive DEFAULT 1,

        CONSTRAINT PK_BranchServices
            PRIMARY KEY (BranchServiceId),

        CONSTRAINT FK_BranchServices_Branches
            FOREIGN KEY (BranchId)
            REFERENCES dbo.Branches(BranchId)
            ON DELETE NO ACTION,

        CONSTRAINT FK_BranchServices_Services
            FOREIGN KEY (ServiceId)
            REFERENCES dbo.Services(ServiceId)
            ON DELETE NO ACTION,

        CONSTRAINT UQ_BranchServices_Branch_Service
            UNIQUE (BranchId, ServiceId),

        CONSTRAINT CHK_BranchServices_Price
            CHECK (Price >= 0)
    );

    ------------------------------------------------------
    -- 5. APPOINTMENT SLOTS
    ------------------------------------------------------

    CREATE TABLE dbo.AppointmentSlots
    (
        AppointmentSlotId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_AppointmentSlots_Id DEFAULT NEWID(),

        BranchServiceId UNIQUEIDENTIFIER NOT NULL,

        StartDateTimeUtc DATETIME2 NOT NULL,
        EndDateTimeUtc DATETIME2 NOT NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_AppointmentSlots_IsActive DEFAULT 1,

        CreatedDate DATETIME2 NOT NULL
            CONSTRAINT DF_AppointmentSlots_CreatedDate
            DEFAULT SYSUTCDATETIME(),

        UpdatedDate DATETIME2 NULL,

        CONSTRAINT PK_AppointmentSlots
            PRIMARY KEY (AppointmentSlotId),

        CONSTRAINT FK_AppointmentSlots_BranchServices
            FOREIGN KEY (BranchServiceId)
            REFERENCES dbo.BranchServices(BranchServiceId)
            ON DELETE NO ACTION,

        CONSTRAINT CHK_AppointmentSlots_DateRange
            CHECK (EndDateTimeUtc > StartDateTimeUtc),

        CONSTRAINT UQ_AppointmentSlots_Service_Start
            UNIQUE (BranchServiceId, StartDateTimeUtc)
    );

    ------------------------------------------------------
    -- 6. CUSTOMERS
    ------------------------------------------------------

    CREATE TABLE dbo.Customers
    (
        CustomerId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_Customers_Id DEFAULT NEWID(),

        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        EmailAddress NVARCHAR(255) NOT NULL,
        PhoneNumber NVARCHAR(50) NOT NULL,

        CreatedDate DATETIME2 NOT NULL
            CONSTRAINT DF_Customers_CreatedDate
            DEFAULT SYSUTCDATETIME(),

        UpdatedDate DATETIME2 NULL,

        CONSTRAINT PK_Customers
            PRIMARY KEY (CustomerId),

        CONSTRAINT UQ_Customers_EmailAddress
            UNIQUE (EmailAddress)
    );

    ------------------------------------------------------
    -- 7. APPOINTMENTS
    ------------------------------------------------------

    CREATE TABLE dbo.Appointments
    (
        AppointmentId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_Appointments_Id DEFAULT NEWID(),

        AppointmentReference NVARCHAR(100) NOT NULL,

        AppointmentSlotId UNIQUEIDENTIFIER NOT NULL,
        CustomerId UNIQUEIDENTIFIER NOT NULL,

        -- Stored as int to match C# enum.
        -- Status-dependent checks must match the application's enum values.
        Status INT NOT NULL
            CONSTRAINT DF_Appointments_Status DEFAULT 0,

        CreatedDate DATETIME2 NOT NULL
            CONSTRAINT DF_Appointments_CreatedDate
            DEFAULT SYSUTCDATETIME(),

        Notes NVARCHAR(2000) NULL,

        ManagementTokenHash NVARCHAR(512) NOT NULL,

        UpdatedDate DATETIME2 NULL,
        CancelledDate DATETIME2 NULL,

        CONSTRAINT PK_Appointments
            PRIMARY KEY (AppointmentId),

        CONSTRAINT FK_Appointments_AppointmentSlots
            FOREIGN KEY (AppointmentSlotId)
            REFERENCES dbo.AppointmentSlots(AppointmentSlotId)
            ON DELETE NO ACTION,

        CONSTRAINT FK_Appointments_Customers
            FOREIGN KEY (CustomerId)
            REFERENCES dbo.Customers(CustomerId)
            ON DELETE NO ACTION,

        CONSTRAINT UQ_Appointments_Reference
            UNIQUE (AppointmentReference)
    );

    ------------------------------------------------------
    -- 8. NOTIFICATIONS
    -- ON DELETE CASCADE retained from original schema.
    ------------------------------------------------------

    CREATE TABLE dbo.Notifications
    (
        NotificationId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_Notifications_Id DEFAULT NEWID(),

        AppointmentId UNIQUEIDENTIFIER NOT NULL,

        -- C# enum values persisted as integers
        NotificationType INT NOT NULL,
        Channel INT NOT NULL,

        Recipient NVARCHAR(255) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,

        Status INT NOT NULL
            CONSTRAINT DF_Notifications_Status DEFAULT 0,

        CreatedDate DATETIME2 NOT NULL
            CONSTRAINT DF_Notifications_CreatedDate
            DEFAULT SYSUTCDATETIME(),

        SentDate DATETIME2 NULL,
        ErrorMessage NVARCHAR(4000) NULL,

        CONSTRAINT PK_Notifications
            PRIMARY KEY (NotificationId),

        CONSTRAINT FK_Notifications_Appointments
            FOREIGN KEY (AppointmentId)
            REFERENCES dbo.Appointments(AppointmentId)
            ON DELETE CASCADE
    );

    ------------------------------------------------------
    -- 9. INDEXES
    -- Prioritize common query predicates over standalone BIT indexes.
    -- Existing UNIQUE constraints already index BranchCode,
    -- EmailAddress, (BranchId, ServiceId),
    -- (BranchServiceId, StartDateTimeUtc), and AppointmentReference.
    ------------------------------------------------------

    CREATE INDEX IX_BranchServices_ServiceId
        ON dbo.BranchServices(ServiceId, IsActive)
        INCLUDE (BranchId, Price);

    CREATE INDEX IX_AppointmentSlots_Availability
        ON dbo.AppointmentSlots(BranchServiceId, IsActive, StartDateTimeUtc)
        INCLUDE (EndDateTimeUtc);

    CREATE INDEX IX_AppointmentSlots_StartDateTimeUtc
        ON dbo.AppointmentSlots(StartDateTimeUtc)
        INCLUDE (EndDateTimeUtc, BranchServiceId, IsActive);

    CREATE INDEX IX_Appointments_CustomerId_CreatedDate
        ON dbo.Appointments(CustomerId, CreatedDate DESC)
        INCLUDE (AppointmentReference, AppointmentSlotId, Status);

    CREATE INDEX IX_Appointments_AppointmentSlotId_Status
        ON dbo.Appointments(AppointmentSlotId, Status);

    CREATE INDEX IX_Appointments_Status_CreatedDate
        ON dbo.Appointments(Status, CreatedDate DESC);

    CREATE INDEX IX_Notifications_AppointmentId
        ON dbo.Notifications(AppointmentId, CreatedDate DESC);

    CREATE INDEX IX_Notifications_Status_CreatedDate
        ON dbo.Notifications(Status, CreatedDate)
        INCLUDE (NotificationType, Channel);

    ------------------------------------------------------
    -- 10. OPTIONAL SAMPLE SEED DATA (development only)
    ------------------------------------------------------

    IF @IncludeSampleData = 1
    BEGIN
    DECLARE @BranchId UNIQUEIDENTIFIER = NEWID();
    DECLARE @ServiceId UNIQUEIDENTIFIER = NEWID();
    DECLARE @BranchServiceId UNIQUEIDENTIFIER = NEWID();
    DECLARE @AppointmentSlotId UNIQUEIDENTIFIER = NEWID();
    DECLARE @CustomerId UNIQUEIDENTIFIER = NEWID();
    DECLARE @AppointmentId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO dbo.Branches
    (
        BranchId,
        BranchName,
        BranchCode,
        TimeZoneId,
        Address,
        City,
        Province,
        ContactNumber
    )
    VALUES
    (
        @BranchId,
        'Sandton Branch',
        'SAND001',
        'South Africa Standard Time',
        '123 Rivonia Road',
        'Sandton',
        'Gauteng',
        '+27110000000'
    );

    INSERT INTO dbo.Services
    (
        ServiceId,
        ServiceName,
        Description
    )
    VALUES
    (
        @ServiceId,
        'Account Consultation',
        'Customer account consultation service'
    );

    INSERT INTO dbo.BranchServices
    (
        BranchServiceId,
        BranchId,
        ServiceId,
        Price
    )
    VALUES
    (
        @BranchServiceId,
        @BranchId,
        @ServiceId,
        0.00
    );

    INSERT INTO dbo.AppointmentSlots
    (
        AppointmentSlotId,
        BranchServiceId,
        StartDateTimeUtc,
        EndDateTimeUtc
    )
    VALUES
    (
        @AppointmentSlotId,
        @BranchServiceId,
        '2026-11-02T08:00:00',
        '2026-11-02T08:30:00'
    );

    INSERT INTO dbo.Customers
    (
        CustomerId,
        FirstName,
        LastName,
        EmailAddress,
        PhoneNumber
    )
    VALUES
    (
        @CustomerId,
        'John',
        'Smith',
        'john.smith@example.com',
        '+27820000000'
    );

    -- Sample uses Status = 0 (assumed Pending).
    -- Token hash is a development-only placeholder.
    INSERT INTO dbo.Appointments
    (
        AppointmentId,
        AppointmentReference,
        AppointmentSlotId,
        CustomerId,
        Status,
        Notes,
        ManagementTokenHash
    )
    VALUES
    (
        @AppointmentId,
        'APT-2026-000001',
        @AppointmentSlotId,
        @CustomerId,
        0,
        'Customer requested account assistance.',
        'DEV-PLACEHOLDER-NOT-A-REAL-TOKEN-HASH'
    );

    -- Notification enum values below are illustrative.
    -- Replace them with actual enum values.
    INSERT INTO dbo.Notifications
    (
        NotificationId,
        AppointmentId,
        NotificationType,
        Channel,
        Recipient,
        Message,
        Status
    )
    VALUES
    (
        NEWID(),
        @AppointmentId,
        0,
        0,
        'john.smith@example.com',
        'Your appointment request has been received.',
        0
    );

    END; -- End optional sample data.

    COMMIT TRANSACTION;

    PRINT 'SUCCESS: BookingSystem schema created with constraints and indexes.';

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();

    PRINT 'ERROR: ' + @ErrorMessage;

    THROW;

END CATCH;

------------------------------------------------------
-- 11. VERIFY TABLE RECORD COUNTS (read-only)
------------------------------------------------------

SELECT 'Branches' AS Entity, COUNT(*) AS RecordCount
FROM dbo.Branches
UNION ALL
SELECT 'Services', COUNT(*) FROM dbo.Services
UNION ALL
SELECT 'BranchServices', COUNT(*) FROM dbo.BranchServices
UNION ALL
SELECT 'AppointmentSlots', COUNT(*) FROM dbo.AppointmentSlots
UNION ALL
SELECT 'Customers', COUNT(*) FROM dbo.Customers
UNION ALL
SELECT 'Appointments', COUNT(*) FROM dbo.Appointments
UNION ALL
SELECT 'Notifications', COUNT(*) FROM dbo.Notifications;

------------------------------------------------------
-- 12. VERIFY APPOINTMENT RELATIONSHIPS (read-only)
------------------------------------------------------

SELECT
    a.AppointmentId,
    a.AppointmentReference,
    c.FirstName,
    c.LastName,
    b.BranchName,
    s.ServiceName,
    aps.StartDateTimeUtc,
    aps.EndDateTimeUtc,
    a.Status,
    a.CreatedDate
FROM dbo.Appointments a
INNER JOIN dbo.Customers c
    ON a.CustomerId = c.CustomerId
INNER JOIN dbo.AppointmentSlots aps
    ON a.AppointmentSlotId = aps.AppointmentSlotId
INNER JOIN dbo.BranchServices bs
    ON aps.BranchServiceId = bs.BranchServiceId
INNER JOIN dbo.Branches b
    ON bs.BranchId = b.BranchId
INNER JOIN dbo.Services s
    ON bs.ServiceId = s.ServiceId;

/*
ADDITIONAL APPLICATION-LEVEL RULES (not encoded without enum definitions):
1. Use a transaction with appropriate locking when reserving an appointment slot.
   Ensure only one ACTIVE booking per slot; canceled bookings may be historical.
2. Reject overlapping time windows for the same branch/service as appropriate.
   A UNIQUE start-time constraint alone does not prevent overlaps.
3. Keep slot times in UTC; convert using Branches.TimeZoneId at API/UI edges.
4. Do not store plaintext management tokens. Store only secure token hashes.
5. Add a filtered unique index for active appointments only after confirming
   the application's exact Status enum values (Pending/Confirmed/Cancelled).
6. For an existing populated database, deploy an ALTER-based migration instead
   of rerunning this full-create script.
*/
