using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingSystem_Explore.Infrastructure.Migrations
{
    public partial class MakeManagementTokenHashNotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure existing NULLs are replaced with empty string before altering column
            migrationBuilder.Sql("UPDATE dbo.Appointments SET ManagementTokenHash = '' WHERE ManagementTokenHash IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "ManagementTokenHash",
                table: "Appointments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ManagementTokenHash",
                table: "Appointments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);
        }
    }
}
