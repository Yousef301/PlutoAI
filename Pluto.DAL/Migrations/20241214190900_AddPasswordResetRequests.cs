using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pluto.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordResetRequest_Users_UserId",
                table: "PasswordResetRequest");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordResetRequest",
                table: "PasswordResetRequest");

            migrationBuilder.RenameTable(
                name: "PasswordResetRequest",
                newName: "PasswordResetRequests");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordResetRequest_UserId",
                table: "PasswordResetRequests",
                newName: "IX_PasswordResetRequests_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordResetRequests",
                table: "PasswordResetRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordResetRequests_Users_UserId",
                table: "PasswordResetRequests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordResetRequests_Users_UserId",
                table: "PasswordResetRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordResetRequests",
                table: "PasswordResetRequests");

            migrationBuilder.RenameTable(
                name: "PasswordResetRequests",
                newName: "PasswordResetRequest");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordResetRequests_UserId",
                table: "PasswordResetRequest",
                newName: "IX_PasswordResetRequest_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordResetRequest",
                table: "PasswordResetRequest",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordResetRequest_Users_UserId",
                table: "PasswordResetRequest",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
