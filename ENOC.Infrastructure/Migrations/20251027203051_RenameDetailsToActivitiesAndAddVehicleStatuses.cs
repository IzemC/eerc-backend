using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ENOC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDetailsToActivitiesAndAddVehicleStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftsReportForm_Incidentes_IncidentId",
                table: "ShiftsReportForm");

            migrationBuilder.DropIndex(
                name: "IX_ShiftsReportForm_IncidentId",
                table: "ShiftsReportForm");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "ShiftsReportForm");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "ShiftsReportForm");

            migrationBuilder.AddColumn<string>(
                name: "Activities",
                table: "ShiftsReportForm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ShiftReportVehicleStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftReportFormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftReportVehicleStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftReportVehicleStatuses_ShiftsReportForm_ShiftReportFormId",
                        column: x => x.ShiftReportFormId,
                        principalTable: "ShiftsReportForm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftReportVehicleStatuses_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftReportVehicleStatuses_ShiftReportFormId",
                table: "ShiftReportVehicleStatuses",
                column: "ShiftReportFormId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftReportVehicleStatuses_VehicleId",
                table: "ShiftReportVehicleStatuses",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftReportVehicleStatuses");

            migrationBuilder.DropColumn(
                name: "Activities",
                table: "ShiftsReportForm");

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "ShiftsReportForm",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentId",
                table: "ShiftsReportForm",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftsReportForm_IncidentId",
                table: "ShiftsReportForm",
                column: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShiftsReportForm_Incidentes_IncidentId",
                table: "ShiftsReportForm",
                column: "IncidentId",
                principalTable: "Incidentes",
                principalColumn: "Id");
        }
    }
}
