using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArogyaPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusAndNotesToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorNotes",
                table: "Patients",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Patients",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorNotes",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Patients");
        }
    }
}
