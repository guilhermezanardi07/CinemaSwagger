using System.Reflection;
using CinemaSwagger.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Padrao")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Cinema API", Version = "v1", Description =
        "API de filmes, sessões, reservas e disponibilidade.\n\n" +
        "### Como usar a parte de Reservas\n" +
        "Uma **reserva** guarda lugares de um cliente em uma sessão de cinema. O caminho normal é:\n\n" +
        "1. Descubra o `id` da sessão em `GET /api/Sessoes`.\n" +
        "2. Crie a reserva em `POST /api/Reservas`, informando `sessaoId`, nome, e-mail e quantidade de lugares (de 1 a 10). " +
        "A API calcula o valor total e só aceita se a sessão ainda não começou e se houver lugares livres.\n" +
        "3. Consulte em `GET /api/Reservas` (filtros: `sessaoId`, `email` e `status`) ou em `GET /api/Reservas/{id}`.\n" +
        "4. Se o cliente mudar de ideia: `PATCH /api/Reservas/{id}/cancelar` libera os lugares e mantém o registro; " +
        "`PATCH /api/Reservas/{id}/reativar` desfaz o cancelamento; `PUT` altera os dados; `DELETE` apaga de vez."
    });
    o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();