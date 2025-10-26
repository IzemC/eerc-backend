using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ENOC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedTeamsAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Teams
            var blackTeamId = Guid.NewGuid();
            var redTeamId = Guid.NewGuid();
            var whiteTeamId = Guid.NewGuid();
            var greenTeamId = Guid.NewGuid();

            migrationBuilder.InsertData(
                table: "Teams",
                columns: new[] { "Id", "Name", "IsDeleted" },
                values: new object[,]
                {
                    { blackTeamId, "Black", false },
                    { redTeamId, "Red", false },
                    { whiteTeamId, "White", false },
                    { greenTeamId, "Green", false }
                });

            // Seed Roles
            var eercRoleId = Guid.NewGuid();
            var managementRoleId = Guid.NewGuid();

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[,]
                {
                    { eercRoleId, "EERC", "EERC", Guid.NewGuid().ToString() },
                    { managementRoleId, "Management", "MANAGEMENT", Guid.NewGuid().ToString() }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Teams",
                keyColumn: "Name",
                keyValues: new object[] { "Black", "Red", "White", "Green" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Name",
                keyValues: new object[] { "EERC", "Management" });
        }
    }
}
