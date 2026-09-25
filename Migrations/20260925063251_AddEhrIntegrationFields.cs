using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NepalMediHub.Migrations
{
    /// <inheritdoc />
    public partial class AddEhrIntegrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PatientHealthId",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrescriptionFileUrl",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientHealthId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientHealthId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PrescriptionFileUrl",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PatientHealthId",
                table: "AspNetUsers");
        }
    }
}
