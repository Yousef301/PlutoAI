using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pluto.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddModelToMessagesAndRemoveFromSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "Sessions");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "Messages");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
