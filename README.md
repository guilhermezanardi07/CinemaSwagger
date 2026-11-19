# CinemaSwagger — API de Cinema

Trabalho final de Técnicas Avançadas de Programação. API REST em ASP.NET Core (.NET 10) com PostgreSQL e documentação Swagger.

**Integrantes:** Arthur F., Bruno S., Guilherme Z., Rafael D., Matheus B.

## Módulos
| Módulo | Rota base | O que faz |
|---|---|---|
| Filmes | `/api/Filmes` | CRUD + `PATCH /{id}/em-cartaz` (regra de negócio) |
| Sessões | `/api/Sessoes` | CRUD; impede sobreposição de horário na mesma sala |
| Reservas | `/api/Reservas` | CRUD + `PATCH /{id}/cancelar` e `/{id}/reativar` (regras de negócio) |
| Disponibilidade | `/api/Disponibilidade` | Consulta de lugares livres por sessão |

Relações: Filme 1—N Sessão 1—N Reserva. Disponibilidade é calculada a partir de Sessão e Reservas Confirmadas.

## Requisitos
- .NET SDK 10
- PostgreSQL (padrão do projeto: porta 5433)

## Configuração
Ajuste a connection string em `appsettings.json` (`ConnectionStrings:Padrao`):
```
Host=localhost;Port=SUA_PORTA;Database=cinema;Username=SEU_USUARIO;Password=SUA_SENHA
```

## Executando
```bash
dotnet restore
dotnet run
```
As migrations são aplicadas automaticamente na inicialização (o banco `cinema` é criado e recebe dados de exemplo: 3 filmes, 3 sessões e 2 reservas).
Abra o Swagger em `http://localhost:5254/swagger`.

## Regras principais
- Reserva: 1 a 10 lugares; só em sessão que ainda não começou e com lugares livres (409 caso contrário).
- Filme com sessões não pode ser excluído (409). Excluir uma sessão exclui as reservas dela.
- Datas em UTC com `Z` (ex.: `2026-12-15T22:30:00Z`).
- Códigos: 200, 201, 204, 400 (validação), 404 (não existe), 409 (regra de negócio).
