using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicaApp.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaProfissional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Users_UserId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Profissionais");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Profissionais",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Profissionais_UserId",
                table: "Profissionais",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Profissionais_Users_UserId",
                table: "Profissionais",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Users_UserId",
                table: "Reservas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profissionais_Users_UserId",
                table: "Profissionais");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Users_UserId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Profissionais_UserId",
                table: "Profissionais");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Profissionais");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Profissionais",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "Profissionais",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Profissionais",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Users_UserId",
                table: "Reservas",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
