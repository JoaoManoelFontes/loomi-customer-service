# Customer Service

## 1. Visão geral

Breve explicação do serviço.

Exemplo:
O Customer Service é responsável pelo gerenciamento de usuários/clientes, autenticação, autorização, dados cadastrais, dados bancários e foto de perfil.

## 2. Responsabilidades do serviço

- Gerenciar clientes
- Gerenciar usuários de autenticação
- Validar credenciais
- Emitir JWT
- Aplicar RBAC
- Consultar dados de cliente
- Atualizar dados cadastrais
- Atualizar foto de perfil
- Armazenar URL da imagem do cliente
- Cachear consultas de cliente com Redis
- Publicar eventos relacionados a alterações de dados bancários

## 3. Tecnologias utilizadas

- .NET 8
- C# 12
- ASP.NET Core Web API
- PostgreSQL ou SQL Server
- Entity Framework Core
- Redis
- Azure Blob Storage
- RabbitMQ ou Azure Service Bus
- Docker
- Docker Compose
- Swagger/OpenAPI
- FluentValidation
- xUnit/NUnit
- Testcontainers, se usar

## 4. Arquitetura interna

Explicar a estrutura do projeto.

Exemplo:

```text
CustomerService.Api
CustomerService.Application
CustomerService.Domain
CustomerService.Infrastructure
CustomerService.Tests

Explicar brevemente:

Api: controllers, middlewares, configurações HTTP
Application: casos de uso, DTOs, handlers, validações
Domain: entidades, value objects, regras de domínio
Infrastructure: banco, cache, blob storage, mensageria, autenticação
Tests: testes unitários e integração
5. Modelo de dados

Explicar as principais entidades:

User
Customer
BankingDetails
ProfilePicture
Role

Exemplo:

### User

Responsável pelos dados de autenticação.

Campos principais:
- Id
- Cpf
- PasswordHash
- Role
- CreatedAt
- UpdatedAt

### Customer

Responsável pelos dados cadastrais e bancários.

Campos principais:
- Id
- UserId
- Name
- Email
- Address
- Agency
- AccountNumber
- ProfilePictureUrl
6. Autenticação e autorização

Explicar:

Como o login funciona
Como o JWT é gerado
Quais roles existem
Quais endpoints exigem autenticação
Quais endpoints exigem role específica

Exemplo de roles:

Admin
Customer
7. Endpoints

Separar por grupos.

### Auth

POST /api/v1/auth/login

### Users

POST /api/v1/users/admin
POST /api/v1/users/customer

### Customers

GET /api/v1/customers/{customerId}
PATCH /api/v1/customers/{customerId}
POST /api/v1/customers/profile-picture/upload-url
PATCH /api/v1/customers/profile-picture

Para cada endpoint, documentar:

Método
URL
Autenticação necessária
Role necessária
Request
Response
Possíveis erros
8. Cache com Redis

Explicar:

Quais dados são cacheados
Por que esses dados foram escolhidos
Qual o tempo de expiração
Quando o cache é invalidado

Exemplo:

O cache foi aplicado na consulta de detalhes do cliente, pois esses dados são lidos com frequência e não mudam constantemente.

Chave:
customer:{customerId}:details

TTL:
10 minutos

Invalidação:
Ao atualizar dados cadastrais, bancários ou foto de perfil.
9. Upload de foto com Azure Blob Storage

Explicar:

Por que usou Blob Storage
Se o upload é feito pela API ou via pre-signed/SAS URL
Como a URL final é salva no banco
Quais validações existem

Exemplo:

A API gera uma URL temporária para upload direto no Azure Blob Storage. Após o upload, o cliente confirma a URL da imagem, e o serviço salva apenas a referência no banco.
10. Mensageria

Explicar eventos publicados pelo serviço.

Exemplo:

### Evento: BankingDetailsUpdated

Publicado quando os dados bancários de um cliente são alterados.

Payload:

{
  "customerId": "uuid",
  "agency": "0001",
  "accountNumber": "123456",
  "occurredAt": "2026-05-24T10:00:00Z"
}

Esse ponto é importante porque o desafio cita uso de broker para notificações sobre atualização dos dados bancários.

11. Tratamento de erros

Explicar o padrão usado.

Exemplo:

ProblemDetails
Exceptions customizadas
Middleware global de erro
Erros de validação
Erros de autenticação/autorização
12. Validações

Explicar uso de FluentValidation ou outra biblioteca.

Exemplo:

As entradas da API são validadas com FluentValidation antes de chegar nos casos de uso.
13. Variáveis de ambiente

Tabela com:

| Variável | Descrição | Exemplo |
|---|---|---|
| ConnectionStrings__Database | String de conexão do banco | Host=localhost... |
| Redis__ConnectionString | Conexão com Redis | localhost:6379 |
| Jwt__Secret | Chave de assinatura JWT | example |
| AzureBlobStorage__ConnectionString | Conexão Azure Blob | example |
| MessageBroker__Host | Host do RabbitMQ | localhost |
14. Como executar localmente

Explicar:

docker compose up -d
dotnet ef database update
dotnet run --project src/CustomerService.Api

Ou, se tudo estiver no Docker:

docker compose up --build
15. Como rodar os testes
dotnet test

Separar se tiver:

dotnet test tests/CustomerService.UnitTests
dotnet test tests/CustomerService.IntegrationTests
16. Swagger/OpenAPI

Informar URL:

http://localhost:5001/swagger
17. Decisões técnicas

Exemplos:

Separação entre User e Customer
Uso de Redis para leitura de detalhes do cliente
Uso de Azure Blob Storage para imagem
Uso de evento assíncrono para alteração de dados bancários
Uso de Clean Architecture
18. Melhorias futuras

Exemplos:

Refresh token
Auditoria de alterações cadastrais
Antivírus/validação avançada para upload
Rate limiting
Observabilidade completa com OpenTelemetry
