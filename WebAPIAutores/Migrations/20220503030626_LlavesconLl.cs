using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPIAutores.Migrations
{
    public partial class LlavesconLl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LavesAPI_AspNetUsers_UsuarioId",
                table: "LavesAPI");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LavesAPI",
                table: "LavesAPI");

            migrationBuilder.RenameTable(
                name: "LavesAPI",
                newName: "LlavesAPI");

            migrationBuilder.RenameIndex(
                name: "IX_LavesAPI_UsuarioId",
                table: "LlavesAPI",
                newName: "IX_LlavesAPI_UsuarioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LlavesAPI",
                table: "LlavesAPI",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LlavesAPI_AspNetUsers_UsuarioId",
                table: "LlavesAPI",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LlavesAPI_AspNetUsers_UsuarioId",
                table: "LlavesAPI");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LlavesAPI",
                table: "LlavesAPI");

            migrationBuilder.RenameTable(
                name: "LlavesAPI",
                newName: "LavesAPI");

            migrationBuilder.RenameIndex(
                name: "IX_LlavesAPI_UsuarioId",
                table: "LavesAPI",
                newName: "IX_LavesAPI_UsuarioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LavesAPI",
                table: "LavesAPI",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LavesAPI_AspNetUsers_UsuarioId",
                table: "LavesAPI",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
