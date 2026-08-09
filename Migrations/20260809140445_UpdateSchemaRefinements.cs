using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArogyaPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaRefinements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SyncLogs_DeviceId",
                table: "SyncLogs");

            migrationBuilder.DropColumn(
                name: "LocalRecordId",
                table: "Patients");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_CreatedAt",
                table: "SyncLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_DeviceId_LocalRecordId",
                table: "SyncLogs",
                columns: new[] { "DeviceId", "LocalRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_RiskLevel",
                table: "Patients",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Timestamp",
                table: "Patients",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Village",
                table: "Patients",
                column: "Village");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Patients_PatientId",
                table: "AuditLogs",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Patients_PatientId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_SyncLogs_CreatedAt",
                table: "SyncLogs");

            migrationBuilder.DropIndex(
                name: "IX_SyncLogs_DeviceId_LocalRecordId",
                table: "SyncLogs");

            migrationBuilder.DropIndex(
                name: "IX_Patients_RiskLevel",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_Timestamp",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_Village",
                table: "Patients");

            migrationBuilder.AddColumn<string>(
                name: "LocalRecordId",
                table: "Patients",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_DeviceId",
                table: "SyncLogs",
                column: "DeviceId");
        }
    }
}
