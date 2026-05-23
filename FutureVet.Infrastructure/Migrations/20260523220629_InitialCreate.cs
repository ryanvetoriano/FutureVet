using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutureVet.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_USUARIO",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Nome = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    Senha = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Cpf = table.Column<string>(type: "NVARCHAR2(11)", maxLength: 11, nullable: false),
                    Telefone = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Disponivel = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_USUARIO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_PET",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NomePet = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Especie = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Raca = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Idade = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Tamanho = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Peso = table.Column<decimal>(type: "DECIMAL(10,2)", precision: 10, scale: 2, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Disponivel = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PET", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TB_PET_TB_USUARIO_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "TB_USUARIO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_CONSULTA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    TipoConsulta = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Data = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    Hora = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    Local = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    PetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Disponivel = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_CONSULTA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TB_CONSULTA_TB_PET_PetId",
                        column: x => x.PetId,
                        principalTable: "TB_PET",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TB_VACINA",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    NomeVacina = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DataAplicacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ProximaDose = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    LocalAplicacao = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    PetId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Disponivel = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_VACINA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TB_VACINA_TB_PET_PetId",
                        column: x => x.PetId,
                        principalTable: "TB_PET",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_CONSULTA_PetId",
                table: "TB_CONSULTA",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_TB_PET_UsuarioId",
                table: "TB_PET",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_Cpf",
                table: "TB_USUARIO",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_USUARIO_Email",
                table: "TB_USUARIO",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_VACINA_PetId",
                table: "TB_VACINA",
                column: "PetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_CONSULTA");

            migrationBuilder.DropTable(
                name: "TB_VACINA");

            migrationBuilder.DropTable(
                name: "TB_PET");

            migrationBuilder.DropTable(
                name: "TB_USUARIO");
        }
    }
}
