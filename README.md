# Cinema API

API REST de cinema desenvolvida em **ASP.NET Core (.NET 10)** com **PostgreSQL** e documentação interativa via **Swagger (OpenAPI)**.
Trabalho final da disciplina **Técnicas Avançadas de Programação**.

## Funcionalidades

- **Filmes:** cadastro, consulta, edição e exclusão; filtro por gênero e situação; colocar/tirar de cartaz.
- **Sessões:** horários dos filmes nas salas.
- **Reservas:** reserva de lugares em uma sessão.
- **Disponibilidade:** lugares livres por sessão.

> Relacionamentos: `Filme` 1 → N `Sessao` 1 → N `Reserva`.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) em execução
- Ferramenta do Entity Framework:

```bash
dotnet tool install --global dotnet-ef
```

## Configuração

1. Clone o repositório:

```bash
git clone https://github.com/guilhermezanardi07/CinemaSwagger.git
cd CinemaSwagger/CinemaSwagger
```

2. Ajuste a conexão com o banco em `appsettings.json` (use o seu host, porta e senha):

```json
"ConnectionStrings": {
  "Padrao": "Host=localhost;Port=5432;Database=cinema;Username=postgres;Password=SUA_SENHA"
}
```

> O banco `cinema` é criado automaticamente. Não envie sua senha pessoal para o repositório.

## Como executar

```bash
dotnet restore
dotnet run
```

Ao iniciar, a aplicação aplica as **migrations** automaticamente, criando as tabelas e inserindo os **dados de exemplo**.

Acesse o Swagger no navegador (a porta aparece no terminal, na linha `Now listening on`):

```
http://localhost:5254/swagger
```

## Migrations

As migrations ficam na pasta `Migrations/`. Para criar uma nova após alterar os models:

```bash
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

## Endpoints

### Filmes (`/api/Filmes`)

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/api/Filmes` | Lista filmes (filtros `genero` e `emCartaz`) | 200 |
| GET | `/api/Filmes/{id}` | Busca filme por Id | 200, 404 |
| POST | `/api/Filmes` | Cadastra filme | 201, 400 |
| PUT | `/api/Filmes/{id}` | Atualiza filme | 204, 400, 404 |
| DELETE | `/api/Filmes/{id}` | Exclui filme | 204, 404 |
| PATCH | `/api/Filmes/{id}/em-cartaz?valor=true` | **Regra de negócio:** coloca ou tira de cartaz | 200, 404 |

### Sessões, Reservas e Disponibilidade

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| GET | `/api/Sessoes` | Lista sessões (filtro opcional `filmeId`) | 200, 400 |
| GET | `/api/Sessoes/{id}` | Consulta uma sessão | 200, 404 |
| POST | `/api/Sessoes` | Cadastra sessão | 201, 400, 404, 409 |
| PUT | `/api/Sessoes/{id}` | Atualiza sessão | 204, 400, 404, 409 |
| DELETE | `/api/Sessoes/{id}` | Exclui sessão | 204, 404 |
| GET | `/api/sessoes/{sessaoId}/disponibilidade` | Consulta capacidade, assentos ocupados e lugares livres | 200, 404 |
| POST | `/api/Reservas` | Reserva assento livre (`sessaoId`, `assento`) | 201, 400, 404, 409 |
| GET | `/api/Reservas?sessaoId=1` | Lista reservas da sessão | 200, 400 |
| DELETE | `/api/Reservas/{id}` | Cancela reserva e libera o assento | 204, 404 |

Os assentos são numerados de `1` até a capacidade da sessão. A soma dos lugares ocupados e disponíveis corresponde à capacidade da sessão. Uma restrição única no banco impede duplicidade mesmo em reservas simultâneas.

Exemplo de resposta de `GET /api/sessoes/1/disponibilidade`:

```json
{
  "sessaoId": 1,
  "capacidade": 80,
  "lugaresOcupados": 1,
  "lugaresDisponiveis": 79,
  "assentosOcupados": ["12"]
}
```

## Exemplo de requisição

`POST /api/Filmes`

```json
{
  "titulo": "Interestelar",
  "genero": "Ficção científica",
  "duracaoMinutos": 169,
  "classificacao": "12 anos",
  "sinopse": "Exploradores viajam por um buraco de minhoca.",
  "dataLancamento": "2014-11-06",
  "emCartaz": true
}
```

## Validações e códigos HTTP

| Código | Significado |
|---|---|
| 200 | Sucesso (consulta ou ação) |
| 201 | Recurso criado |
| 204 | Sucesso sem conteúdo (atualização/exclusão) |
| 400 | Dados inválidos (ex.: título vazio, duração fora de 1 a 600) |
| 404 | Recurso não encontrado |

## Dados de exemplo

Ao subir a API, o banco já vem com filmes cadastrados (Interestelar, Divertida Mente e O Poderoso Chefão) para testes e para a apresentação.

## Estrutura do projeto

```
CinemaSwagger/
├── Controllers/    # Endpoints da API
├── Data/           # AppDbContext e seed
├── Models/         # Entidades e DTOs
├── Migrations/     # Histórico do banco
├── Program.cs      # Configuração (EF Core, Swagger)
└── appsettings.json
```

## Tecnologias

ASP.NET Core · Entity Framework Core · PostgreSQL (Npgsql) · Swashbuckle (Swagger UI)

## Equipe

Grupo: Arthur Lima, Bruno Seleguin, Guilherme Zanardi, Matheus Bernardes, Rafael Dias
