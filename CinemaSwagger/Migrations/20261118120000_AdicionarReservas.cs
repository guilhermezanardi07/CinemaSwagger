using CinemaSwagger.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaSwagger.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261118120000_AdicionarReservas")]
public partial class AdicionarReservas : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Reservas",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SessaoId = table.Column<int>(type: "integer", nullable: false),
                Assento = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                CriadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reservas", x => x.Id);
                table.ForeignKey("FK_Reservas_Sessoes_SessaoId", x => x.SessaoId, "Sessoes", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_Reservas_SessaoId_Assento", "Reservas", new[] { "SessaoId", "Assento" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "Reservas");
}
