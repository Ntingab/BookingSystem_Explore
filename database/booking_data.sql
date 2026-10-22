/*
 BookingSystem_Explore — SQL Server schema and repeatable demo seed
 Defaults: no destructive reset; insert sample records.
 First run: creates all seven tables. Later runs: leaves existing tables intact
 and adds only missing seeded sample records.
 IMPORTANT: Setting @AllowDestructiveReset=1 deletes the seven tables and data.
*/
USE [master];
GO
IF DB_ID(N'BookingSystem') IS NULL
BEGIN
    CREATE DATABASE [BookingSystem];
    PRINT 'Created BookingSystem database.';
END;
GO
USE [BookingSystem];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @AllowDestructiveReset BIT = 0; -- Set to 1 ONLY to delete/recreate tables.
DECLARE @IncludeSampleData BIT = 1;     -- Demo records enabled by default.
DECLARE @ExistingCount INT;

BEGIN TRY
    BEGIN TRANSACTION;

    SELECT @ExistingCount = COUNT(*)
    FROM sys.tables
    WHERE schema_id = SCHEMA_ID(N'dbo')
      AND name IN (N'Branches',N'Services',N'BranchServices',N'AppointmentSlots',
                   N'Customers',N'Appointments',N'Notifications');

    IF @AllowDestructiveReset = 1
    BEGIN
        IF OBJECT_ID(N'dbo.Notifications',N'U') IS NOT NULL DROP TABLE dbo.Notifications;
        IF OBJECT_ID(N'dbo.Appointments',N'U') IS NOT NULL DROP TABLE dbo.Appointments;
        IF OBJECT_ID(N'dbo.AppointmentSlots',N'U') IS NOT NULL DROP TABLE dbo.AppointmentSlots;
        IF OBJECT_ID(N'dbo.BranchServices',N'U') IS NOT NULL DROP TABLE dbo.BranchServices;
        IF OBJECT_ID(N'dbo.Customers',N'U') IS NOT NULL DROP TABLE dbo.Customers;
        IF OBJECT_ID(N'dbo.Services',N'U') IS NOT NULL DROP TABLE dbo.Services;
        IF OBJECT_ID(N'dbo.Branches',N'U') IS NOT NULL DROP TABLE dbo.Branches;
        SET @ExistingCount = 0;
        PRINT 'Reset: seven BookingSystem tables dropped.';
    END
    ELSE IF @ExistingCount NOT IN (0,7)
        THROW 51000, 'Partial BookingSystem schema found. No changes made. Use an ALTER-based migration or inspect tables before resetting.', 1;

    IF @ExistingCount = 0
    BEGIN
        CREATE TABLE dbo.Branches
        (
            BranchId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Branches_Id DEFAULT NEWID(),
            BranchName NVARCHAR(200) NOT NULL,
            BranchCode NVARCHAR(50) NOT NULL,
            TimeZoneId NVARCHAR(100) NOT NULL CONSTRAINT DF_Branches_TimeZone DEFAULT N'UTC',
            Address NVARCHAR(500) NOT NULL,
            City NVARCHAR(150) NOT NULL,
            Province NVARCHAR(150) NOT NULL,
            ContactNumber NVARCHAR(50) NOT NULL,
            IsActive BIT NOT NULL CONSTRAINT DF_Branches_IsActive DEFAULT (1),
            CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Branches_CreatedDate DEFAULT SYSUTCDATETIME(),
            UpdatedDate DATETIME2 NULL,
            CONSTRAINT PK_Branches PRIMARY KEY (BranchId),
            CONSTRAINT UQ_Branches_BranchCode UNIQUE (BranchCode)
        );

        CREATE TABLE dbo.Services
        (
            ServiceId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Services_Id DEFAULT NEWID(),
            ServiceName NVARCHAR(200) NOT NULL,
            Description NVARCHAR(1000) NULL,
            DurationMinutes INT NOT NULL CONSTRAINT DF_Services_DurationMinutes DEFAULT (30),
            IsActive BIT NOT NULL CONSTRAINT DF_Services_IsActive DEFAULT (1),
            CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Services_CreatedDate DEFAULT SYSUTCDATETIME(),
            UpdatedDate DATETIME2 NULL,
            CONSTRAINT PK_Services PRIMARY KEY (ServiceId),
            CONSTRAINT CHK_Services_DurationMinutes CHECK (DurationMinutes > 0)
        );

        CREATE TABLE dbo.BranchServices
        (
            BranchServiceId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BranchServices_Id DEFAULT NEWID(),
            BranchId UNIQUEIDENTIFIER NOT NULL,
            ServiceId UNIQUEIDENTIFIER NOT NULL,
            Price DECIMAL(18,2) NOT NULL CONSTRAINT DF_BranchServices_Price DEFAULT (0),
            CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_BranchServices_CreatedDate DEFAULT SYSUTCDATETIME(),
            UpdatedDate DATETIME2 NULL,
            IsActive BIT NOT NULL CONSTRAINT DF_BranchServices_IsActive DEFAULT (1),
            CONSTRAINT PK_BranchServices PRIMARY KEY (BranchServiceId),
            CONSTRAINT FK_BranchServices_Branches FOREIGN KEY (BranchId) REFERENCES dbo.Branches(BranchId) ON DELETE NO ACTION,
            CONSTRAINT FK_BranchServices_Services FOREIGN KEY (ServiceId) REFERENCES dbo.Services(ServiceId) ON DELETE NO ACTION,
            CONSTRAINT UQ_BranchServices_Branch_Service UNIQUE (BranchId, ServiceId),
            CONSTRAINT CHK_BranchServices_Price CHECK (Price >= 0)
        );

        CREATE TABLE dbo.AppointmentSlots
        (
            AppointmentSlotId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AppointmentSlots_Id DEFAULT NEWID(),
            BranchServiceId UNIQUEIDENTIFIER NOT NULL,
            StartDateTimeUtc DATETIME2 NOT NULL,
            EndDateTimeUtc DATETIME2 NOT NULL,
            IsActive BIT NOT NULL CONSTRAINT DF_AppointmentSlots_IsActive DEFAULT (1),
            CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_AppointmentSlots_CreatedDate DEFAULT SYSUTCDATETIME(),
            UpdatedDate DATETIME2 NULL,
            CONSTRAINT PK_AppointmentSlots PRIMARY KEY (AppointmentSlotId),
            CONSTRAINT FK_AppointmentSlots_BranchServices FOREIGN KEY (BranchServiceId) REFERENCES dbo.BranchServices(BranchServiceId) ON DELETE NO ACTION,
            CONSTRAINT CHK_AppointmentSlots_DateRange CHECK (EndDateTimeUtc > StartDateTimeUtc),
            CONSTRAINT UQ_AppointmentSlots_Service_Start UNIQUE (BranchServiceId, StartDateTimeUtc)
        );

        CREATE TABLE dbo.Customers
        (
            CustomerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Customers_Id DEFAULT NEWID(),
            FirstName NVARCHAR(100) NOT NULL,
            LastName NVARCHAR(100) NOT NULL,
            EmailAddress NVARCHAR(255) NOT NULL,
            PhoneNumber NVARCHAR(50) NOT NULL,
            CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Customers_CreatedDate DEFAULT SYSUTCDATETIME(),
            UpdatedDate DATETIME2 NULL,
            CONSTRAINT PK_Customers PRIMARY KEY (CustomerId),
            CONSTRAINT UQ_Customers_EmailAddress UNIQUE (EmailAddress)
        );

        CREATE TABLE dbo.Appointments
        (
            AppointmentId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Appointments_Id DEFAULT NEWID(),
            AppointmentReference NVARCHAR(100) NOT NULL,
            AppointmentSlotId UNIQUEIDENTIFIER NOT NULL,
            CustomerId UNIQUEIDENTIFIER NOT NULL,
            Status INT NOT NULL CONSTRAINT DF_Appointments_Status DEFAULT (0),
            CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Appointments_CreatedDate DEFAULT SYSUTCDATETIME(),
            Notes NVARCHAR(2000) NULL,
            ManagementTokenHash NVARCHAR(512) NOT NULL,
            UpdatedDate DATETIME2 NULL,
            CancelledDate DATETIME2 NULL,
            CONSTRAINT PK_Appointments PRIMARY KEY (AppointmentId),
            CONSTRAINT FK_Appointments_AppointmentSlots FOREIGN KEY (AppointmentSlotId) REFERENCES dbo.AppointmentSlots(AppointmentSlotId) ON DELETE NO ACTION,
            CONSTRAINT FK_Appointments_Customers FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId) ON DELETE NO ACTION,
            CONSTRAINT UQ_Appointments_Reference UNIQUE (AppointmentReference)
        );

        CREATE TABLE dbo.Notifications
        (
            NotificationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Notifications_Id DEFAULT NEWID(),
            AppointmentId UNIQUEIDENTIFIER NOT NULL,
            NotificationType INT NOT NULL,
            Channel INT NOT NULL,
            Recipient NVARCHAR(255) NOT NULL,
            Message NVARCHAR(MAX) NOT NULL,
            Status INT NOT NULL CONSTRAINT DF_Notifications_Status DEFAULT (0),
            CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_Notifications_CreatedDate DEFAULT SYSUTCDATETIME(),
            SentDate DATETIME2 NULL,
            ErrorMessage NVARCHAR(4000) NULL,
            CONSTRAINT PK_Notifications PRIMARY KEY (NotificationId),
            CONSTRAINT FK_Notifications_Appointments FOREIGN KEY (AppointmentId) REFERENCES dbo.Appointments(AppointmentId) ON DELETE CASCADE
        );

        CREATE INDEX IX_BranchServices_ServiceId ON dbo.BranchServices(ServiceId, IsActive) INCLUDE (BranchId, Price);
        CREATE INDEX IX_AppointmentSlots_Availability ON dbo.AppointmentSlots(BranchServiceId, IsActive, StartDateTimeUtc) INCLUDE (EndDateTimeUtc);
        CREATE INDEX IX_AppointmentSlots_StartDateTimeUtc ON dbo.AppointmentSlots(StartDateTimeUtc) INCLUDE (EndDateTimeUtc, BranchServiceId, IsActive);
        CREATE INDEX IX_Appointments_CustomerId_CreatedDate ON dbo.Appointments(CustomerId, CreatedDate DESC) INCLUDE (AppointmentReference, AppointmentSlotId, Status);
        CREATE INDEX IX_Appointments_AppointmentSlotId_Status ON dbo.Appointments(AppointmentSlotId, Status);
        CREATE INDEX IX_Appointments_Status_CreatedDate ON dbo.Appointments(Status, CreatedDate DESC);
        CREATE INDEX IX_Notifications_AppointmentId ON dbo.Notifications(AppointmentId, CreatedDate DESC);
        CREATE INDEX IX_Notifications_Status_CreatedDate ON dbo.Notifications(Status, CreatedDate) INCLUDE (NotificationType, Channel);
        PRINT 'Created seven tables and eight indexes.';
    END
    ELSE
        PRINT 'All seven tables already exist; keeping existing schema/data unchanged.';

    IF @IncludeSampleData = 1
    BEGIN
        /* Fixed seed IDs make re-running the script safe (no duplicate dummy records).
           These are DEVELOPMENT records, not real customers or credentials. */
        DECLARE @Branches TABLE (Id UNIQUEIDENTIFIER, Name NVARCHAR(200), Code NVARCHAR(50), Address NVARCHAR(500), City NVARCHAR(150), Province NVARCHAR(150), Phone NVARCHAR(50));
        INSERT INTO @Branches VALUES
        ('5b49ebfd-a47c-4e6f-bba3-be3016b1f5db',N'Sandton Branch',N'DEMO-SAND001',N'123 Rivonia Road',N'Sandton',N'Gauteng',N'+27110000001'),
        ('6F8C2A91-D3B5-4E70-9A62-C18F4B7D25E9',N'Midrand Branch',N'DEMO-MID001',N'45 Main Road',N'Midrand',N'Gauteng',N'+27110000002'),
        ('A7D3F291-6B4E-4C85-9F12-E0B7A63D2984',N'Pretoria Branch',N'DEMO-PTA001',N'10 Church Street',N'Pretoria',N'Gauteng',N'+27120000003');
        INSERT INTO dbo.Branches (BranchId,BranchName,BranchCode,TimeZoneId,Address,City,Province,ContactNumber)
        SELECT b.Id,b.Name,b.Code,N'South Africa Standard Time',b.Address,b.City,b.Province,b.Phone
        FROM @Branches b
        WHERE NOT EXISTS(SELECT 1 FROM dbo.Branches x WITH (UPDLOCK,HOLDLOCK) WHERE x.BranchId=b.Id OR x.BranchCode=b.Code);

        DECLARE @Services TABLE (Id UNIQUEIDENTIFIER, Name NVARCHAR(200), Description NVARCHAR(1000), Duration INT);
        INSERT INTO @Services VALUES
        ('8B42F6A1-73C9-4E0D-A5B2-19F8D6C3E740',N'Account Consultation',N'Customer account assistance',30),
        ('C7E91A35-4D82-4B6F-9A13-52D0F8E7B469',N'Financial Advisory',N'Financial planning consultation',60),
        ('2F6D8B40-A1C7-4E95-B3F2-76A9D0C8415E',N'Document Verification',N'Verify submitted customer documents',20),
        ('D5A37E92-8B14-4F60-A9C5-31E2B7D8640F',N'General Enquiry',N'General help desk appointment',30);
        INSERT INTO dbo.Services (ServiceId,ServiceName,Description,DurationMinutes)
        SELECT s.Id,s.Name,s.Description,s.Duration FROM @Services s
        WHERE NOT EXISTS(SELECT 1 FROM dbo.Services x WITH (UPDLOCK,HOLDLOCK) WHERE x.ServiceId=s.Id);

        DECLARE @BranchServices TABLE (Id UNIQUEIDENTIFIER, BranchId UNIQUEIDENTIFIER, ServiceId UNIQUEIDENTIFIER, Price DECIMAL(18,2));
        INSERT INTO @BranchServices VALUES
        ('F2A9C714-6E35-4B80-9D42-A7F1E83C6509','5b49ebfd-a47c-4e6f-bba3-be3016b1f5db','8B42F6A1-73C9-4E0D-A5B2-19F8D6C3E740',0.00),
        ('A6F2D981-4B73-4E05-9C12-F8D7B3406A95','5b49ebfd-a47c-4e6f-bba3-be3016b1f5db','C7E91A35-4D82-4B6F-9A13-52D0F8E7B469',350.00),
        ('B8E41D65-2F93-4A70-8C15-D6A9F327E084','6F8C2A91-D3B5-4E70-9A62-C18F4B7D25E9','2F6D8B40-A1C7-4E95-B3F2-76A9D0C8415E',0.00),
        ('3C9F72A6-8D41-4E05-B6A3-F217C8D9540E','6F8C2A91-D3B5-4E70-9A62-C18F4B7D25E9','D5A37E92-8B14-4F60-A9C5-31E2B7D8640F',0.00),
        ('E5A28B73-9C16-4F40-A7D2-63B9E1F805CA','A7D3F291-6B4E-4C85-9F12-E0B7A63D2984','8B42F6A1-73C9-4E0D-A5B2-19F8D6C3E740',0.00);
        INSERT INTO dbo.BranchServices (BranchServiceId,BranchId,ServiceId,Price)
        SELECT bs.Id,bs.BranchId,bs.ServiceId,bs.Price FROM @BranchServices bs
        WHERE EXISTS(SELECT 1 FROM dbo.Branches b WHERE b.BranchId=bs.BranchId)
          AND EXISTS(SELECT 1 FROM dbo.Services s WHERE s.ServiceId=bs.ServiceId)
          AND NOT EXISTS(SELECT 1 FROM dbo.BranchServices x WITH (UPDLOCK,HOLDLOCK)
                         WHERE x.BranchServiceId=bs.Id OR (x.BranchId=bs.BranchId AND x.ServiceId=bs.ServiceId));

        DECLARE @Slots TABLE (Id UNIQUEIDENTIFIER, BranchServiceId UNIQUEIDENTIFIER, Starts DATETIME2, Ends DATETIME2);
        INSERT INTO @Slots VALUES
        ('C9E51A72-8F34-4D60-B2A7-16F0E9C583DB','F2A9C714-6E35-4B80-9D42-A7F1E83C6509','2026-11-02T08:00:00','2026-11-02T08:30:00'),
        ('7B3D8F26-A190-4C75-8E42-D5A9F6310B84','F2A9C714-6E35-4B80-9D42-A7F1E83C6509','2026-11-02T08:30:00','2026-11-02T09:00:00'),
        ('E4A76C93-2D85-4F10-A9B3-70C1D8E6524F','A6F2D981-4B73-4E05-9C12-F8D7B3406A95','2026-11-02T09:00:00','2026-11-02T10:00:00'),
        ('2F8C5B71-6A39-4D84-B0E7-93A1F6C425D8','A6F2D981-4B73-4E05-9C12-F8D7B3406A95','2026-11-02T10:00:00','2026-11-02T11:00:00'),
        ('9A7D3F21-B6C4-4E85-8F12-D0A6B93C574E','B8E41D65-2F93-4A70-8C15-D6A9F327E084','2026-11-03T08:00:00','2026-11-03T08:20:00'),
        ('F4B82C69-1D75-4A30-9E6B-C2F718D59403','B8E41D65-2F93-4A70-8C15-D6A9F327E084','2026-11-03T08:20:00','2026-11-03T08:40:00'),
        ('6E1A9D54-C837-4B26-A5F0-92D7E3418C6B','3C9F72A6-8D41-4E05-B6A3-F217C8D9540E','2026-11-04T12:00:00','2026-11-04T12:30:00'),
        ('B3F70A28-5C91-4D64-8A2E-F619C0B735D4','E5A28B73-9C16-4F40-A7D2-63B9E1F805CA','2026-11-05T08:00:00','2026-11-05T08:30:00');
        INSERT INTO dbo.AppointmentSlots (AppointmentSlotId,BranchServiceId,StartDateTimeUtc,EndDateTimeUtc)
        SELECT sl.Id,sl.BranchServiceId,sl.Starts,sl.Ends FROM @Slots sl
        WHERE EXISTS(SELECT 1 FROM dbo.BranchServices bs WHERE bs.BranchServiceId=sl.BranchServiceId)
          AND NOT EXISTS(SELECT 1 FROM dbo.AppointmentSlots x WITH (UPDLOCK,HOLDLOCK)
                         WHERE x.AppointmentSlotId=sl.Id OR (x.BranchServiceId=sl.BranchServiceId AND x.StartDateTimeUtc=sl.Starts));

        DECLARE @Customers TABLE (Id UNIQUEIDENTIFIER, FirstName NVARCHAR(100), LastName NVARCHAR(100), Email NVARCHAR(255), Phone NVARCHAR(50));
        INSERT INTO @Customers VALUES
        ('8C4F2A71-D963-4B05-A7E1-2F90C6D835BA',N'John',N'Smith',N'demo.john.smith@example.com',N'+27820000001'),
        ('F7A31D85-6B42-4E90-8C5F-D219A7B634E0',N'Thando',N'Mokoena',N'demo.thando.mokoena@example.com',N'+27820000002'),
        ('2D9B6F43-A871-4C25-B0E6-7F13D8A954C2',N'Naledi',N'Dlamini',N'demo.naledi.dlamini@example.com',N'+27820000003'),
        ('B5E72A90-3C64-4F18-9D2B-A861E7C4305F',N'Peter',N'Naidoo',N'demo.peter.naidoo@example.com',N'+27820000004');
        INSERT INTO dbo.Customers (CustomerId,FirstName,LastName,EmailAddress,PhoneNumber)
        SELECT c.Id,c.FirstName,c.LastName,c.Email,c.Phone FROM @Customers c
        WHERE NOT EXISTS(SELECT 1 FROM dbo.Customers x WITH (UPDLOCK,HOLDLOCK) WHERE x.CustomerId=c.Id OR x.EmailAddress=c.Email);

        /* Status=0 is an illustrative C# enum value. Replace with your actual enum mapping.
           This seeded hash is a dummy SHA-256 value, NOT a working management token. */
        DECLARE @Appointments TABLE (Id UNIQUEIDENTIFIER, Ref NVARCHAR(100), Slot UNIQUEIDENTIFIER, Customer UNIQUEIDENTIFIER, Status INT, Notes NVARCHAR(2000));
        INSERT INTO @Appointments VALUES
        ('6A1F8D32-E495-4B70-A3C9-F257D0E6184B',N'DEMO-APT-000001','C9E51A72-8F34-4D60-B2A7-16F0E9C583DB','8C4F2A71-D963-4B05-A7E1-2F90C6D835BA',0,N'Sample account consultation'),
        ('D3C79A51-2F86-4E40-B1D5-8A693F0C742E',N'DEMO-APT-000002','E4A76C93-2D85-4F10-A9B3-70C1D8E6524F','F7A31D85-6B42-4E90-8C5F-D219A7B634E0',0,N'Sample financial advisory visit'),
        ('9F2B5E84-C731-4A06-8D92-E416B7C350FA',N'DEMO-APT-000003','9A7D3F21-B6C4-4E85-8F12-D0A6B93C574E','2D9B6F43-A871-4C25-B0E6-7F13D8A954C2',0,N'Sample document verification');
        INSERT INTO dbo.Appointments (AppointmentId,AppointmentReference,AppointmentSlotId,CustomerId,Status,Notes,ManagementTokenHash)
        SELECT a.Id,a.Ref,a.Slot,a.Customer,a.Status,a.Notes,
               CONVERT(NVARCHAR(512),N'DEMO-NONFUNCTIONAL-HASH')
        FROM @Appointments a
        WHERE EXISTS(SELECT 1 FROM dbo.AppointmentSlots sl WHERE sl.AppointmentSlotId=a.Slot)
          AND EXISTS(SELECT 1 FROM dbo.Customers c WHERE c.CustomerId=a.Customer)
          AND NOT EXISTS(SELECT 1 FROM dbo.Appointments x WITH (UPDLOCK,HOLDLOCK) WHERE x.AppointmentId=a.Id OR x.AppointmentReference=a.Ref);

        DECLARE @Notifications TABLE (Id UNIQUEIDENTIFIER, AppointmentId UNIQUEIDENTIFIER, Recipient NVARCHAR(255), Message NVARCHAR(1000));
        INSERT INTO @Notifications VALUES
        ('A4D86C29-7E13-4F95-B6A0-3C82D1E7495F','6A1F8D32-E495-4B70-A3C9-F257D0E6184B',N'demo.john.smith@example.com',N'Your account consultation has been received.'),
        ('5B9E1F73-D264-4C80-A7B3-F918E0D6524C','D3C79A51-2F86-4E40-B1D5-8A693F0C742E',N'demo.thando.mokoena@example.com',N'Your financial advisory appointment has been received.'),
        ('E7A42D96-8C35-4B71-9F20-D6E183A5C947','9F2B5E84-C731-4A06-8D92-E416B7C350FA',N'demo.naledi.dlamini@example.com',N'Your document verification appointment has been received.');
        INSERT INTO dbo.Notifications (NotificationId,AppointmentId,NotificationType,Channel,Recipient,Message,Status)
        SELECT n.Id,n.AppointmentId,0,0,n.Recipient,n.Message,0
        FROM @Notifications n
        WHERE EXISTS(SELECT 1 FROM dbo.Appointments a WHERE a.AppointmentId=n.AppointmentId)
          AND NOT EXISTS(SELECT 1 FROM dbo.Notifications x WITH (UPDLOCK,HOLDLOCK) WHERE x.NotificationId=n.Id);
        PRINT 'Demo seed applied (only missing sample records inserted).';
    END;

    COMMIT TRANSACTION;
    PRINT 'SUCCESS: BookingSystem schema and demo seed completed.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO

-- Verification: record counts
SELECT N'Branches' AS Entity,COUNT(*) AS RecordCount FROM dbo.Branches
UNION ALL SELECT N'Services',COUNT(*) FROM dbo.Services
UNION ALL SELECT N'BranchServices',COUNT(*) FROM dbo.BranchServices
UNION ALL SELECT N'AppointmentSlots',COUNT(*) FROM dbo.AppointmentSlots
UNION ALL SELECT N'Customers',COUNT(*) FROM dbo.Customers
UNION ALL SELECT N'Appointments',COUNT(*) FROM dbo.Appointments
UNION ALL SELECT N'Notifications',COUNT(*) FROM dbo.Notifications;

-- Verification: appointment details, with related branch/service/customer.
SELECT a.AppointmentId,a.AppointmentReference,c.FirstName,c.LastName,
       b.BranchName,s.ServiceName,aps.StartDateTimeUtc,aps.EndDateTimeUtc,
       a.Status,a.CreatedDate
FROM dbo.Appointments a
JOIN dbo.Customers c ON c.CustomerId=a.CustomerId
JOIN dbo.AppointmentSlots aps ON aps.AppointmentSlotId=a.AppointmentSlotId
JOIN dbo.BranchServices bs ON bs.BranchServiceId=aps.BranchServiceId
JOIN dbo.Branches b ON b.BranchId=bs.BranchId
JOIN dbo.Services s ON s.ServiceId=bs.ServiceId
ORDER BY aps.StartDateTimeUtc;
GO
/* Application concerns: enforce one active booking per slot using a transaction and
   a filtered unique index after you define actual C# status enum values;
   explicitly check overlapping slots; convert UTC to the branch's Windows time zone;
   never use seeded placeholder management tokens for authentication.
   Existing schemas are NOT migrated or index-altered by this bootstrap script. */
