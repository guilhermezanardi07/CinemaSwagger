using CinemaSwagger.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaSwagger.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Filme> Filmes => Set<Filme>();
    // Colegas: DbSet<Sessao> Sessoes, DbSet<Reserva> Reservas...

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Filme>().HasData(
            new Filme { Id = 1, Titulo = "Interestelar", Genero = "Ficção científica", DuracaoMinutos = 169, Classificacao = "12 anos", Sinopse = "Exploradores viajam por um buraco de minhoca.", DataLancamento = new DateOnly(2014, 11, 6), EmCartaz = true },
            new Filme { Id = 2, Titulo = "Divertida Mente", Genero = "Animação", DuracaoMinutos = 95, Classificacao = "Livre", Sinopse = "As emoções de uma garota ganham vida.", DataLancamento = new DateOnly(2015, 6, 18), EmCartaz = true },
            new Filme { Id = 3, Titulo = "O Poderoso Chefão", Genero = "Drama", DuracaoMinutos = 175, Classificacao = "16 anos", Sinopse = "A saga de uma família mafiosa.", DataLancamento = new DateOnly(1972, 3, 24), EmCartaz = false }
        );
    }
}