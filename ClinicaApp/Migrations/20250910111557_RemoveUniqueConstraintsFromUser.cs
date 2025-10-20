using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraintsFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove índice único de Email
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            // Cria índice normal (não único) de Email
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            // **Não mexer no Telefone**
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverte índice de Email
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }
    }
}
