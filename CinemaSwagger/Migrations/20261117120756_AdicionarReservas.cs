using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaSwagger.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessaoId = table.Column<int>(type: "integer", nullable: false),
                    NomeCliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    QuantidadeLugares = table.Column<int>(type: "integer", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataReserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Sessoes_SessaoId",
                        column: x => x.SessaoId,
                        principalTable: "Sessoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Reservas",
                columns: new[] { "Id", "DataReserva", "Email", "NomeCliente", "QuantidadeLugares", "SessaoId", "Status", "ValorTotal" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 11, 10, 14, 0, 0, 0, DateTimeKind.Utc), "ana.souza@email.com", "Ana Souza", 2, 1, "Confirmada", 50.00m },
                    { 2, new DateTime(2026, 11, 11, 10, 30, 0, 0, DateTimeKind.Utc), "carlos.lima@email.com", "Carlos Lima", 4, 2, "Confirmada", 80.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_SessaoId",
                table: "Reservas",
                column: "SessaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservas");
        }
    }
}
