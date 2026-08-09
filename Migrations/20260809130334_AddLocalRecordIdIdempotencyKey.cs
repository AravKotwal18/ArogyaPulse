using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArogyaPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalRecordIdIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocalRecordId",
                table: "SyncLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LocalRecordId",
                table: "Patients",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalRecordId",
                table: "SyncLogs");

            migrationBuilder.DropColumn(
                name: "LocalRecordId",
                table: "Patients");
        }
    }
}
