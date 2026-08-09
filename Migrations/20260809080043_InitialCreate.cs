using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArogyaPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Village = table.Column<string>(type: "TEXT", nullable: false),
                    Bp = table.Column<string>(type: "TEXT", nullable: false),
                    SpO2 = table.Column<int>(type: "INTEGER", nullable: false),
                    Temp = table.Column<double>(type: "REAL", nullable: false),
                    Glucose = table.Column<int>(type: "INTEGER", nullable: false),
                    Symptoms = table.Column<string>(type: "TEXT", nullable: false),
                    IsPregnant = table.Column<bool>(type: "INTEGER", nullable: false),
                    RiskScore = table.Column<int>(type: "INTEGER", nullable: false),
                    RiskLevel = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
