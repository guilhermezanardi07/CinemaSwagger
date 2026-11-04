using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaSwagger.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Filmes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Genero = table.Column<string>(type: "text", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "integer", nullable: false),
                    Classificacao = table.Column<string>(type: "text", nullable: false),
                    Sinopse = table.Column<string>(type: "text", nullable: true),
                    DataLancamento = table.Column<DateOnly>(type: "date", nullable: false),
                    EmCartaz = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filmes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Filmes",
                columns: new[] { "Id", "Classificacao", "DataLancamento", "DuracaoMinutos", "EmCartaz", "Genero", "Sinopse", "Titulo" },
                values: new object[,]
                {
                    { 1, "12 anos", new DateOnly(2014, 11, 6), 169, true, "Ficção científica", "Exploradores viajam por um buraco de minhoca.", "Interestelar" },
                    { 2, "Livre", new DateOnly(2015, 6, 18), 95, true, "Animação", "As emoções de uma garota ganham vida.", "Divertida Mente" },
                    { 3, "16 anos", new DateOnly(1972, 3, 24), 175, false, "Drama", "A saga de uma família mafiosa.", "O Poderoso Chefão" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Filmes");
        }
    }
}
