using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem_Explore.Infrastructure.Migrations
{
    public partial class AddAppointmentReminders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedAppointmentSlotId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppointmentId_RelatedAppointmentSlotId_NotificationType",
                table: "Notifications",
                columns: new[] { "AppointmentId", "RelatedAppointmentSlotId", "NotificationType" },
                unique: true,
                filter: "[RelatedAppointmentSlotId] IS NOT NULL AND [NotificationType] = 3");

            migrationBuilder.Sql("""
                CREATE OR ALTER PROCEDURE dbo.QueueAppointmentReminders
                AS
                BEGIN
                    SET NOCOUNT ON;
                    DECLARE @NowUtc datetime2 = SYSUTCDATETIME();
                    DECLARE @ReminderCutoffUtc datetime2 = DATEADD(HOUR, 24, @NowUtc);

                    INSERT INTO dbo.Notifications
                        (NotificationId, AppointmentId, RelatedAppointmentSlotId, NotificationType, Channel, Recipient, Message, Status, CreatedDate)
                    SELECT
                        NEWID(), a.AppointmentId, a.AppointmentSlotId, 3, 0, c.EmailAddress,
                        CONCAT('Reminder: your appointment is scheduled for ', CONVERT(varchar(33), s.StartDateTimeUtc, 126), ' UTC.'),
                        0, @NowUtc
                    FROM dbo.Appointments a
                    INNER JOIN dbo.AppointmentSlots s ON s.AppointmentSlotId = a.AppointmentSlotId
                    INNER JOIN dbo.Customers c ON c.CustomerId = a.CustomerId
                    WHERE a.Status = 1
                      AND s.StartDateTimeUtc > @NowUtc
                      AND s.StartDateTimeUtc <= @ReminderCutoffUtc
                      AND NOT EXISTS
                      (
                          SELECT 1
                          FROM dbo.Notifications n
                          WHERE n.AppointmentId = a.AppointmentId
                            AND n.RelatedAppointmentSlotId = a.AppointmentSlotId
                            AND n.NotificationType = 3
                      );
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.QueueAppointmentReminders;");
            migrationBuilder.DropIndex(
                name: "IX_Notifications_AppointmentId_RelatedAppointmentSlotId_NotificationType",
                table: "Notifications");
            migrationBuilder.DropColumn(name: "RelatedAppointmentSlotId", table: "Notifications");
        }
    }
}
