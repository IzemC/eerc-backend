using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ENOC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSignaturesToImageFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new signature columns to Inspections
            migrationBuilder.AddColumn<byte[]>(
                name: "UserSignature",
                table: "Inspections",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserSignatureContentType",
                table: "Inspections",
                type: "nvarchar(max)",
                nullable: true);

            // Drop and recreate Signature column for AspNetUsers (can't convert nvarchar to varbinary)
            migrationBuilder.DropColumn(
                name: "Signature",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<byte[]>(
                name: "Signature",
                table: "AspNetUsers",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureContentType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove signature columns from Inspections
            migrationBuilder.DropColumn(
                name: "UserSignature",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "UserSignatureContentType",
                table: "Inspections");

            // Drop and recreate Signature column for AspNetUsers (revert back to string)
            migrationBuilder.DropColumn(
                name: "SignatureContentType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
