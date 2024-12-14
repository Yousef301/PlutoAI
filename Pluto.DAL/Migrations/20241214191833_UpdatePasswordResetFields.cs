using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pluto.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordResetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Used",
                table: "PasswordResetRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Used",
                table: "PasswordResetRequests");
        }
    }
}
