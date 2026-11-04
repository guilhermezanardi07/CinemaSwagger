using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaSwagger.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSessoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sessoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FilmeId = table.Column<int>(type: "integer", nullable: false),
                    Sala = table.Column<string>(type: "text", nullable: false),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Preco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Capacidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessoes_Filmes_FilmeId",
                        column: x => x.FilmeId,
                        principalTable: "Filmes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Sessoes",
                columns: new[] { "Id", "Capacidade", "DataHora", "FilmeId", "Preco", "Sala" },
                values: new object[,]
                {
                    { 1, 80, new DateTime(2026, 12, 15, 22, 30, 0, 0, DateTimeKind.Utc), 1, 25.00m, "Sala 1" },
                    { 2, 100, new DateTime(2026, 12, 15, 19, 0, 0, 0, DateTimeKind.Utc), 2, 20.00m, "Sala 2" },
                    { 3, 60, new DateTime(2026, 12, 16, 22, 0, 0, 0, DateTimeKind.Utc), 3, 30.00m, "Sala 3" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessoes_FilmeId",
                table: "Sessoes",
                column: "FilmeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessoes_Sala_DataHora",
                table: "Sessoes",
                columns: new[] { "Sala", "DataHora" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sessoes");
        }
    }
}
