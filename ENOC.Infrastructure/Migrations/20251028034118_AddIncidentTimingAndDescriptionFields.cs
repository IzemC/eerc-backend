using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ENOC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentTimingAndDescriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Incidentes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOfAllClear",
                table: "Incidentes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOfArrival",
                table: "Incidentes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOfTurnout",
                table: "Incidentes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Incidentes");

            migrationBuilder.DropColumn(
                name: "TimeOfAllClear",
                table: "Incidentes");

            migrationBuilder.DropColumn(
                name: "TimeOfArrival",
                table: "Incidentes");

            migrationBuilder.DropColumn(
                name: "TimeOfTurnout",
                table: "Incidentes");
        }
    }
}
