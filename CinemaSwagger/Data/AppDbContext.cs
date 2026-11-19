using CinemaSwagger.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwagger.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Filme> Filmes => Set<Filme>();
    public DbSet<Sessao> Sessoes => Set<Sessao>();
    public DbSet<Reserva> Reservas => Set<Reserva>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Apenas o módulo de Sessões foi acrescentado ao contexto original.
        mb.Entity<Sessao>()
            .HasOne(s => s.Filme)
            .WithMany()
            .HasForeignKey(s => s.FilmeId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Sessao>().Property(s => s.Preco).HasPrecision(10, 2);
        mb.Entity<Sessao>().HasIndex(s => new { s.Sala, s.DataHora });

        // IDs do seed são 1, 2 e 3; novos IDs gerados começam em 100.
        mb.Entity<Sessao>().Property(s => s.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 100);

        mb.Entity<Sessao>().HasData(
            new Sessao { Id = 1, FilmeId = 1, Sala = "Sala 1", DataHora = DateTime.SpecifyKind(new DateTime(2026, 12, 15, 22, 30, 0), DateTimeKind.Utc), Preco = 25.00m, Capacidade = 80 },
            new Sessao { Id = 2, FilmeId = 2, Sala = "Sala 2", DataHora = DateTime.SpecifyKind(new DateTime(2026, 12, 15, 19, 00, 0), DateTimeKind.Utc), Preco = 20.00m, Capacidade = 100 },
            new Sessao { Id = 3, FilmeId = 3, Sala = "Sala 3", DataHora = DateTime.SpecifyKind(new DateTime(2026, 12, 16, 22, 00, 0), DateTimeKind.Utc), Preco = 30.00m, Capacidade = 60 }
        );

        // Módulo de Reservas: excluir uma sessão remove também as reservas dela.
        mb.Entity<Reserva>()
            .HasOne(r => r.Sessao)
            .WithMany()
            .HasForeignKey(r => r.SessaoId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Reserva>().Property(r => r.NomeCliente).HasMaxLength(100);
        mb.Entity<Reserva>().Property(r => r.Email).HasMaxLength(150);
        mb.Entity<Reserva>().Property(r => r.ValorTotal).HasPrecision(10, 2);
        mb.Entity<Reserva>().Property(r => r.Status).HasMaxLength(20);

        // IDs do seed são 1 e 2; novos IDs gerados começam em 100.
        mb.Entity<Reserva>().Property(r => r.Id)
            .UseIdentityByDefaultColumn()
            .HasIdentityOptions(startValue: 100);

        mb.Entity<Reserva>().HasData(
            new Reserva { Id = 1, SessaoId = 1, NomeCliente = "Ana Souza", Email = "ana.souza@email.com", QuantidadeLugares = 2, ValorTotal = 50.00m, Status = StatusReserva.Confirmada, DataReserva = DateTime.SpecifyKind(new DateTime(2026, 11, 10, 14, 0, 0), DateTimeKind.Utc) },
            new Reserva { Id = 2, SessaoId = 2, NomeCliente = "Carlos Lima", Email = "carlos.lima@email.com", QuantidadeLugares = 4, ValorTotal = 80.00m, Status = StatusReserva.Confirmada, DataReserva = DateTime.SpecifyKind(new DateTime(2026, 11, 11, 10, 30, 0), DateTimeKind.Utc) }
        );

        // Seed original de Filmes mantido sem alterações.
        mb.Entity<Filme>().HasData(
            new Filme { Id = 1, Titulo = "Interestelar", Genero = "Ficção científica", DuracaoMinutos = 169, Classificacao = "12 anos", Sinopse = "Exploradores viajam por um buraco de minhoca.", DataLancamento = new DateOnly(2014, 11, 6), EmCartaz = true },
            new Filme { Id = 2, Titulo = "Divertida Mente", Genero = "Animação", DuracaoMinutos = 95, Classificacao = "Livre", Sinopse = "As emoções de uma garota ganham vida.", DataLancamento = new DateOnly(2015, 6, 18), EmCartaz = true },
            new Filme { Id = 3, Titulo = "O Poderoso Chefão", Genero = "Drama", DuracaoMinutos = 175, Classificacao = "16 anos", Sinopse = "A saga de uma família mafiosa.", DataLancamento = new DateOnly(1972, 3, 24), EmCartaz = false }
        );
    }
}
