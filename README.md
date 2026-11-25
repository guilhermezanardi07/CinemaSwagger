# 🎬 CinemaSwagger — Sistema de Gerenciamento de Cinema

Sistema de gerenciamento de cinema desenvolvido como trabalho final da disciplina de **Técnicas Avançadas de Programação**.

O projeto é composto por duas aplicações integradas: uma API REST, responsável pelo processamento das operações e gerenciamento dos dados, e uma aplicação web, responsável pela interface de interação com os usuários.

## 👥 Equipe de desenvolvimento

- Arthur F.
- Bruno S.
- Guilherme Z.
- Matheus B.
- Rafael D.

## 🏗️ Estrutura do projeto

O sistema está dividido em dois projetos independentes que trabalham em conjunto.

| Projeto | Descrição | Endereço local |
|---|---|---|
| CinemaSwagger | Backend: API REST, regras de negócio, persistência e documentação Swagger. | `http://localhost:5254` |
| CinemaWeb | Frontend: interface web que consome os serviços disponibilizados pela API. | `http://localhost:5262` |

### 1. CinemaSwagger — Backend

Desenvolvido em C# com ASP.NET Core (.NET 10), é responsável pelo processamento das requisições, aplicação das regras de negócio e comunicação com o banco de dados PostgreSQL.

**Tecnologias:**
- C#
- ASP.NET Core (.NET 10)
- Entity Framework Core
- PostgreSQL
- Swagger / OpenAPI

### 2. CinemaWeb — Frontend

Aplicação web responsável pela interface visual do sistema. Consome os endpoints disponibilizados pelo CinemaSwagger para consultar, cadastrar, atualizar e excluir informações.

A comunicação entre as aplicações acontece por meio de requisições HTTP.

## 🎟️ Funcionalidades

O sistema possui quatro módulos principais:

| Módulo | Rota base | Funcionalidades |
|---|---|---|
| Filmes | `/api/Filmes` | CRUD completo e gerenciamento de filmes em cartaz. |
| Sessões | `/api/Sessoes` | CRUD completo e validação de conflitos de horários na mesma sala. |
| Reservas | `/api/Reservas` | CRUD completo, cancelamento e reativação de reservas. |
| Disponibilidade | `/api/Disponibilidade` | Consulta de lugares disponíveis por sessão. |

### Relacionamentos entre entidades

- **Filme (1:N) Sessão:** um filme pode possuir várias sessões.
- **Sessão (1:N) Reserva:** uma sessão pode possuir várias reservas.
- **Disponibilidade:** calculada com base nas sessões e nas reservas confirmadas.

## ⚙️ Requisitos

Para executar o sistema, são necessários:

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- Navegador web atualizado.

## 🗄️ Configuração do banco de dados

O backend utiliza PostgreSQL para armazenar as informações do sistema.

A connection string deve ser configurada no arquivo `CinemaSwagger/appsettings.json`, na propriedade `ConnectionStrings:Padrao`.

Exemplo de configuração:

```text
Host=localhost;Port=5433;Database=cinema;Username=SEU_USUARIO;Password=SUA_SENHA
```

Substitua os valores conforme as configurações do PostgreSQL instalado no ambiente.

**Importante:** não publique senhas ou credenciais reais no repositório.

As migrations são aplicadas automaticamente durante a inicialização da API.

O sistema também disponibiliza dados iniciais para facilitar os testes e a demonstração:

- 3 filmes;
- 3 sessões;
- 2 reservas.

## 🚀 Como executar o sistema

**Atenção:** as duas aplicações precisam estar em execução simultaneamente. O CinemaSwagger deve ser iniciado primeiro, pois o CinemaWeb depende da API para funcionar corretamente.

### 1. Iniciar o CinemaSwagger (Backend)

Abra um terminal na pasta principal do repositório e execute:

```bash
cd CinemaSwagger/CinemaSwagger
dotnet restore
dotnet run --launch-profile http
```

A API estará disponível em:

`http://localhost:5254`

A documentação interativa do Swagger estará disponível em:

`http://localhost:5254/swagger`

Mantenha a aplicação em execução.

### 2. Iniciar o CinemaWeb (Frontend)

Abra um segundo terminal, sem encerrar o backend.

Acesse a pasta do projeto CinemaWeb e execute:

```bash
cd CinemaWeb
dotnet restore
dotnet run --launch-profile http
```

A interface web estará disponível em:

`http://localhost:5262`

Mantenha as duas aplicações em execução simultaneamente.

**Observação:** caso a estrutura interna do CinemaWeb coloque o arquivo `.csproj` em uma subpasta, execute os comandos a partir da pasta que contém esse arquivo.

### 3. Acessar o sistema

Com as duas aplicações iniciadas, abra o navegador e acesse:

**http://localhost:5262**

A partir da interface web, o usuário poderá utilizar as funcionalidades do sistema. As operações serão encaminhadas ao CinemaSwagger, que processará as requisições e realizará as operações necessárias no banco de dados.

## 📖 Documentação Swagger

O Swagger disponibiliza a documentação interativa da API, permitindo visualizar e testar os endpoints diretamente pelo navegador.

A documentação apresenta as operações disponíveis, seus parâmetros, dados de requisição e respostas HTTP.

**Endereço:**

http://localhost:5254/swagger

## 📋 Regras de negócio

### Reservas
- Permite reservar de 1 a 10 lugares por operação.
- Não permite reservas em sessões que já começaram.
- Verifica a disponibilidade dos lugares antes de confirmar uma reserva.
- Retorna `409 Conflict` quando uma regra de negócio impede a operação.

### Filmes e sessões
- Não permite excluir filmes que possuam sessões vinculadas.
- Ao excluir uma sessão, suas reservas também são excluídas.
- Impede a sobreposição de horários de sessões na mesma sala.

### Datas
As datas devem ser informadas em UTC, utilizando o sufixo `Z`.

Exemplo:

```text
2026-12-15T22:30:00Z
```

## 📡 Códigos de resposta HTTP

| Código | Descrição |
|---|---|
| `200 OK` | Requisição realizada com sucesso. |
| `201 Created` | Recurso criado com sucesso. |
| `204 No Content` | Operação concluída sem conteúdo de retorno. |
| `400 Bad Request` | Requisição inválida ou erro de validação. |
| `404 Not Found` | Recurso não encontrado. |
| `409 Conflict` | Conflito com uma regra de negócio. |

## 📦 Entrega

O repositório contém os projetos responsáveis pelo funcionamento integrado do sistema:

- **CinemaSwagger:** backend e API REST.
- **CinemaWeb:** frontend e interface web.

O projeto inclui configurações, código-fonte, documentação e migrations necessárias para a execução e demonstração da aplicação.

---

**CinemaSwagger & CinemaWeb**

Trabalho final da disciplina de Técnicas Avançadas de Programação.