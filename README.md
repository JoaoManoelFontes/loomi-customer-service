# Customer Service

Microserviço ASP.NET Core Web API para gerenciamento de clientes em um desafio de banking/microservices.

O serviço concentra dados cadastrais, dados bancários, saldo, autenticação simples por JWT, autorização por roles, cache Redis para leituras de cliente e geração de URL temporária para upload de foto de perfil em Blob Storage.

> Observação: o projeto atual usa PostgreSQL com EF Core/Npgsql. RabbitMQ/mensageria não está implementado no código nem no `docker-compose.yml`.

## 1. Visão geral

O `CustomerService` é responsável por:

- Autenticar usuários por CPF e senha.
- Emitir JWT.
- Aplicar autorização por roles.
- Criar clientes e usuários associados.
- Consultar dados do cliente autenticado ou por ID, conforme role.
- Atualizar dados cadastrais, bancários e URL da foto de perfil.
- Verificar se um cliente existe.
- Atualizar saldo entre dois clientes dentro da base do `customer-service`.
- Cachear leituras de cliente com Redis.
- Gerar URL temporária de upload para foto de perfil.

O saldo pertence a este serviço porque os dados bancários e o saldo fazem parte do agregado de cliente. Um futuro `transfer-service` deve coordenar transferências chamando este serviço, e não manter ou alterar saldo diretamente em uma base própria.

## Acesso ao Transfer Service

- [Transfer Service - Swagger](http://joaomanoelfontes-loomi-customer.eastus.cloudapp.azure.com/swagger/index.html)
- [Transfer Service - Health Check](http://joaomanoelfontes-loomi-customer.eastus.cloudapp.azure.com/health)

## 2. Tecnologias utilizadas

- .NET `net10.0`
- ASP.NET Core Web API
- C#
- Entity Framework Core
- Npgsql / PostgreSQL
- Redis com `StackExchange.Redis`
- JWT Bearer Authentication
- Autorização por roles
- FluentValidation
- Swagger/OpenAPI com versionamento de API
- Azure Blob Storage SDK
- Application Insights opcional
- Docker e Docker Compose
- xUnit
- FluentAssertions
- Testcontainers para PostgreSQL e Redis nos testes de integração

## 3. Arquitetura interna

```text
customer-service/
  src/
    CustomerService.Api/
    CustomerService.Application/
    CustomerService.Domain/
    CustomerService.Infrastructure/
  tests/
    CustomerService.UnitTests/
    CustomerService.IntegrationTests/
```

### CustomerService.Api

Camada HTTP. Contém controllers versionados, configuração de autenticação/autorização, Swagger, middleware global de exceções, health check e composição de dependências.

Arquivos principais:

- `Program.cs`
- `Controllers/V1/AuthController.cs`
- `Controllers/V1/CustomerController.cs`
- `Middlewares/ExceptionHandlingMiddleware.cs`
- `Extensions/SwaggerExtensions.cs`

### CustomerService.Application

Camada de casos de uso e contratos. Contém handlers, requests/responses, validações e abstrações para repositórios, cache, token, hash de senha e storage.

Exemplos:

- `LoginHandler`
- `CreateUserHandler`
- `GetCustomerDetailsHandler`
- `UpdateCustomerHandler`
- `UpdateBalanceHandler`
- `CreateProfilePictureUploadUrlHandler`

### CustomerService.Domain

Camada de domínio. Não referencia EF Core, ASP.NET, Redis ou Blob Storage.

Entidades atuais:

- `Customer`
- `BankingDetails`
- `User`

### CustomerService.Infrastructure

Implementações externas. Contém EF Core, PostgreSQL, Redis, JWT, hash de senha e Blob Storage.

Exemplos:

- `CustomerDbContext`
- `CustomerRepository`
- `CustomerBalanceTransferRepository`
- `UserRepository`
- `RedisCustomerExistenceCache`
- `RedisCustomerDetailsCache`
- `AzureBlobProfilePictureStorageService`
- `LocalProfilePictureStorageService`

### Tests

- `CustomerService.UnitTests`: testes de domínio e handlers de aplicação.
- `CustomerService.IntegrationTests`: testes HTTP, autenticação/RBAC, Swagger, Redis, upload URL e atualização de saldo.

## 4. Modelo de dados

As migrations atuais criam tabelas para `customers`, `banking_details` e `users`.

### Customer

Representa o cliente e seus dados cadastrais.

Campos principais no domínio:

- `Id`
- `Name`
- `Email`
- `Address`
- `ProfilePictureUrl`
- `BankingDetails`

### BankingDetails

Representa os dados bancários e saldo do cliente.

Campos principais:

- `Id`
- `CustomerId`
- `Agency`
- `CheckingAccountNumber`
- `Balance`

Regras de domínio atuais:

- Agência e conta são obrigatórias.
- Saldo não pode ser negativo.
- Débito exige valor positivo e saldo suficiente.
- Crédito exige valor positivo.

### User

Representa credenciais e autorização.

Campos principais:

- `Id`
- `Cpf`
- `PasswordHash`
- `Role`
- `CustomerId`
- `CreatedAt`
- `UpdatedAt`

Roles existentes em `UserRole`:

- `Admin`
- `Customer`
- `Service`

## 5. Autenticação e autorização

A autenticação usa CPF e senha. O login retorna um JWT que deve ser enviado no header:

```text
Authorization: Bearer <token>
```

O JWT usa:

- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:Secret`
- `Jwt:ExpiresInMinutes`

Claims relevantes:

- `role`: usada pela autorização por roles.
- `customer_id`: usada pelos endpoints de autoatendimento do cliente autenticado.

### Usuário admin de desenvolvimento

Em ambiente `Development`, o projeto chama `DevelopmentAdminSeeder.SeedDevelopmentAdminAsync`.

Credencial local criada para demonstração:

```text
CPF: 00000000000
Senha: Admin@123
Role: Admin
```

Essa credencial é apenas para desenvolvimento e demonstração do desafio.

## 6. Endpoints

Todos os endpoints abaixo estão versionados em `/api/v1`.

### Health

```http
GET /health
```

Não exige autenticação.

Resposta:

```json
{
  "service": "customer-service",
  "status": "healthy",
  "timestamp": "2026-05-27T00:00:00.0000000+00:00"
}
```

### Auth

#### Login

```http
POST /api/v1/auth/login
Content-Type: application/json
```

Request:

```json
{
  "cpf": "00000000000",
  "password": "Admin@123"
}
```

Response:

```json
{
  "accessToken": "<jwt>",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

Possíveis erros:

- `400 Bad Request`
- `401 Unauthorized`

### Customers

#### Criar cliente

```http
POST /api/v1/customers
Authorization: Bearer <admin-token>
Content-Type: application/json
```

Exige role `Admin`.

Request:

```json
{
  "cpf": "12345678900",
  "password": "Customer@123",
  "role": "Customer",
  "name": "Maria Silva",
  "email": "maria@example.com",
  "address": "Rua Exemplo, 100",
  "agency": "0001",
  "checkingAccountNumber": "123456",
  "balance": 1000
}
```

Possíveis erros:

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `409 Conflict` para CPF duplicado.

#### Consultar cliente autenticado

```http
GET /api/v1/customers
Authorization: Bearer <customer-token>
```

Exige role `Customer` e claim `customer_id`.

Response:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "name": "Maria Silva",
  "email": "maria@example.com",
  "address": "Rua Exemplo, 100",
  "profilePictureUrl": null,
  "bankingDetails": {
    "agency": "0001",
    "checkingAccountNumber": "123456",
    "balance": 1000
  }
}
```

Possíveis erros:

- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

#### Consultar cliente por ID

```http
GET /api/v1/customers/{customerId}
Authorization: Bearer <admin-token>
```

Exige role `Admin`.

Possíveis erros:

- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

#### Verificar existência de cliente

```http
GET /api/v1/customers/{customerId}/exists
```

Retorna `true` ou `false`.

No código atual, esse endpoint não possui `[Authorize]` no controller.

#### Atualizar cliente autenticado

```http
PATCH /api/v1/customers
Authorization: Bearer <customer-token>
Content-Type: application/json
```

Exige role `Customer` e claim `customer_id`.

Request parcial. Pelo menos um campo suportado deve ser enviado:

```json
{
  "name": "Maria Souza",
  "email": "maria.souza@example.com",
  "address": "Rua Nova, 200",
  "profileImageUrl": "https://example.com/profile.png",
  "bankingDetails": {
    "agency": "0002",
    "checkingAccountNumber": "654321"
  }
}
```

Response:

```json
{
  "customerId": "00000000-0000-0000-0000-000000000000",
  "status": "updated"
}
```

Possíveis erros:

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`

#### Atualizar cliente por ID

```http
PATCH /api/v1/customers/{customerId}
Authorization: Bearer <admin-token>
Content-Type: application/json
```

Exige role `Admin`.

Usa o mesmo payload parcial de `PATCH /api/v1/customers`.

#### Gerar URL de upload de foto de perfil

```http
POST /api/v1/customers/profile-picture/upload-url
Authorization: Bearer <customer-token>
Content-Type: application/json
```

Exige role `Customer` e claim `customer_id`.

Request:

```json
{
  "fileName": "profile.png",
  "contentType": "image/png",
  "fileSizeInBytes": 102400
}
```

Validações atuais:

- Extensões permitidas: `.jpg`, `.jpeg`, `.png`, `.webp`.
- Content types permitidos: `image/jpeg`, `image/png`, `image/webp`.
- Tamanho máximo: 5 MB.

Quando `AzureBlobStorage:ConnectionString` está vazio, a aplicação usa `LocalProfilePictureStorageService` e retorna URLs placeholder com domínio `https://local.blob-storage.invalid`.

#### Atualizar saldo entre clientes

```http
POST /api/v1/customers/update-balance
Authorization: Bearer <customer-token>
Content-Type: application/json
```

Exige role `Customer` e claim `customer_id`.

Request:

```json
{
  "receiverId": "00000000-0000-0000-0000-000000000000",
  "amount": 100
}
```

O handler usa o cliente autenticado como remetente, debita o remetente e credita o recebedor em uma transação no banco.

Possíveis erros:

- `400 Bad Request`
- `401 Unauthorized`
- `403 Forbidden`
- `404 Not Found`
- `409 Conflict` para saldo insuficiente.

## 7. Cache com Redis

O Redis é usado como otimização. PostgreSQL continua sendo a fonte da verdade.

### Existência de cliente

Endpoint:

```http
GET /api/v1/customers/{customerId}/exists
```

Comportamento:

- Tenta ler do Redis.
- Em cache miss, consulta PostgreSQL.
- Salva `true` ou `false` no Redis.
- Falhas de Redis são ignoradas para manter o endpoint disponível.

TTL:

```text
CustomerCache:ExistsTtlSeconds
```

Valor padrão atual: `300` segundos.

Chave usada pela implementação Redis:

```text
customers:{customerId}:exists
```

### Detalhes de cliente

Endpoints de consulta de detalhes usam `ICustomerDetailsCache`.

Comportamento:

- Tenta ler detalhes do Redis.
- Em cache miss, consulta PostgreSQL.
- Salva a resposta serializada no Redis.
- Atualizações de cliente e saldo regravam o cache de detalhes.
- Falhas de Redis são ignoradas.

TTL:

```text
CustomerCache:DetailsTtlSeconds
```

Valor padrão atual: `300` segundos.

Chave:

```text
customers:{customerId}:details
```

## 8. Upload de foto com Azure Blob Storage

O projeto possui abstração `IProfilePictureStorageService`.

Implementações atuais:

- `AzureBlobProfilePictureStorageService`: usada quando `AzureBlobStorage:ConnectionString` está configurada.
- `LocalProfilePictureStorageService`: fallback local quando a connection string está vazia.

Fluxo atual:

1. Cliente autenticado chama `POST /api/v1/customers/profile-picture/upload-url`.
2. API valida nome, tipo e tamanho do arquivo.
3. API retorna uma URL temporária de upload.
4. O upload esperado é via `PUT`.
5. A URL final pode ser salva no cliente usando `PATCH /api/v1/customers` com `profileImageUrl`.

O código atual gera a URL de upload, mas não faz upload do binário pela API.

## 9. Mensageria

Mensageria não está implementada no código atual.

Não há integração MassTransit/RabbitMQ, não há eventos publicados e o `docker-compose.yml` atual não sobe RabbitMQ.

Evento planejado para uma evolução futura:

```text
CustomerBankingDetailsUpdated
```

Esse evento faria sentido quando dados bancários fossem alterados, para notificar outros serviços de forma assíncrona.

## 10. Tratamento de erros

O projeto usa `ExceptionHandlingMiddleware` para transformar exceções em respostas `ProblemDetails`/`ValidationProblemDetails`.

Mapeamentos atuais:

- `ValidationException`: `400 Bad Request`
- `InvalidCredentialsException`: `401 Unauthorized`
- `DuplicateCpfException`: `409 Conflict`
- `CustomerNotFoundException`: `404 Not Found`
- `InsufficientBalanceException`: `409 Conflict`
- `InvalidBalanceTransferException`: `400 Bad Request`
- `DomainValidationException`: `400 Bad Request`
- Exceções não tratadas: `500 Internal Server Error`

Erros internos não retornam stack trace para o cliente.

## 11. Validações

As entradas são validadas com FluentValidation na camada Application.

Validadores existentes:

- `LoginRequestValidator`
- `CreateUserRequestValidator`
- `UpdateCustomerRequestValidator`
- `CreateProfilePictureUploadUrlRequestValidator`
- `UpdateBalanceRequestValidator`

Exemplos de regras:

- CPF deve conter 11 dígitos.
- Senha deve ter pelo menos 8 caracteres.
- Role deve ser `Admin`, `Customer` ou `Service`.
- E-mail deve ter formato válido.
- Atualização parcial de cliente deve conter pelo menos um campo suportado.
- URL de imagem de perfil deve ser absoluta, HTTP ou HTTPS.
- Upload de imagem aceita JPEG, PNG ou WebP até 5 MB.
- Atualização de saldo exige recebedor e valor maior que zero.

## 12. Observabilidade e logging

O projeto usa logging padrão do ASP.NET Core configurado em `appsettings.json`.

Também existe integração opcional com Application Insights:

- Se `ApplicationInsights:ConnectionString` estiver vazia, a API inicia sem registrar telemetry Azure.
- Se estiver preenchida, a aplicação registra Application Insights.

Não há Serilog configurado no código atual.

## 13. Infraestrutura do serviço

A infraestrutura alvo para executar este serviço em ambiente cloud é baseada em Azure:

- Azure VM para hospedar o `customer-service`.
- Azure Cache for Redis para cache de existência e detalhes de cliente.
- Azure Database for PostgreSQL para persistência relacional.
- Azure Blob Storage para armazenar fotos de perfil de clientes.

Responsabilidades esperadas por componente:

| Componente | Uso no serviço | Configuração principal |
|---|---|---|
| Azure VM | Hospeda a API ASP.NET Core, preferencialmente via container Docker ou serviço gerenciado pelo sistema operacional | Variáveis de ambiente da aplicação e porta HTTP exposta |
| Azure Database for PostgreSQL | Fonte da verdade para `customers`, `banking_details` e `users` | `ConnectionStrings__Postgres` |
| Azure Cache for Redis | Cache de leituras frequentes, como existência e detalhes de cliente | `Redis__ConnectionString` |
| Azure Blob Storage | Geração de URL temporária para upload de foto de perfil | `AzureBlobStorage__ConnectionString` e `AzureBlobStorage__ContainerName` |

Em desenvolvimento local, o `docker-compose.yml` substitui parte dessa infraestrutura com PostgreSQL e Redis locais. Quando `AzureBlobStorage__ConnectionString` está vazia, a aplicação usa o fallback local de storage e não exige uma conta Azure.

Para produção, segredos como connection strings, chave JWT e credenciais de serviços devem ser configurados fora do repositório, por exemplo via variáveis de ambiente da VM, secret manager do pipeline ou Azure Key Vault. O repositório mantém apenas placeholders seguros em `.env.example`.

## 14. Variáveis de ambiente

Exemplo disponível em `.env.example`.

| Variável | Descrição | Exemplo |
|---|---|---|
| `CUSTOMER_API_HTTP_PORT` | Porta HTTP exposta pelo Docker Compose | `5001` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente ASP.NET Core | `Development` |
| `POSTGRES_HOST` | Host do PostgreSQL no Compose | `postgres` |
| `POSTGRES_PORT` | Porta do PostgreSQL | `5432` |
| `POSTGRES_DB` | Nome do banco | `customer_service` |
| `POSTGRES_USER` | Usuário do banco | `customer_service` |
| `POSTGRES_PASSWORD` | Senha local do banco | `customer_service_dev_password` |
| `ConnectionStrings__Postgres` | Connection string usada pela aplicação | `Host=postgres;Port=5432;Database=customer_service;...` |
| `REDIS_HOST` | Host do Redis usado pelo Docker Compose | `redis` |
| `REDIS_PORT` | Porta do Redis | `6379` |
| `Redis__Host` | Host do Redis lido pela aplicação quando não há connection string completa | `redis` |
| `Redis__Port` | Porta do Redis lida pela aplicação quando não há connection string completa | `6379` |
| `Redis__ConnectionString` | Connection string completa do Redis. Se vazia, a aplicação monta com `Redis__Host` e `Redis__Port` | `redis:6379` |
| `CustomerCache__ExistsTtlSeconds` | TTL do cache de existência | `300` |
| `AzureBlobStorage__ConnectionString` | Connection string do Azure Blob Storage | vazio para fallback local |
| `AzureBlobStorage__ContainerName` | Container de imagens | `customer-profile-pictures` |
| `AzureBlobStorage__UploadUrlExpiresInMinutes` | Expiração da URL de upload | `10` |
| `ApplicationInsights__ConnectionString` | Connection string opcional do Application Insights | vazio em local |
| `Jwt__Issuer` | Issuer do JWT | `CustomerService` |
| `Jwt__Audience` | Audience do JWT | `BankingSystem` |
| `Jwt__Secret` | Chave de assinatura do JWT | valor local de desenvolvimento |
| `Jwt__ExpiresInMinutes` | Expiração do token | `60` |

## 15. Como executar localmente

Pré-requisitos:

- .NET SDK compatível com `net10.0`
- Docker Desktop
- `dotnet-ef`, se for aplicar migrations manualmente

Restaurar e compilar:

```bash
dotnet restore
dotnet build
```

Subir dependências locais:

```bash
docker compose up -d postgres redis
```

Aplicar migrations manualmente, se necessário:

```bash
dotnet ef database update \
  --project src/CustomerService.Infrastructure \
  --startup-project src/CustomerService.Api \
  --context CustomerDbContext
```

Rodar a API:

```bash
dotnet run --project src/CustomerService.Api --urls http://localhost:5001
```

Health check:

```text
http://localhost:5001/health
```

Swagger:

```text
http://localhost:5001/swagger
```

## 16. Como executar com Docker Compose

Copie `.env.example` para `.env` e ajuste os valores se necessário.

```bash
docker compose up -d --build
```

Serviços atuais do Compose:

- API: `http://localhost:5001`
- Swagger: `http://localhost:5001/swagger`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`

O Compose atual não inclui RabbitMQ.

## 17. Como rodar os testes

Todos os testes:

```bash
dotnet test
```

Somente testes unitários:

```bash
dotnet test tests/CustomerService.UnitTests
```

Somente testes de integração:

```bash
dotnet test tests/CustomerService.IntegrationTests
```

Os testes de integração usam Testcontainers para PostgreSQL e Redis.

## 18. Migrations

Criar nova migration:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/CustomerService.Infrastructure \
  --startup-project src/CustomerService.Api \
  --context CustomerDbContext \
  --output-dir Persistence/Migrations
```

Aplicar migrations:

```bash
dotnet ef database update \
  --project src/CustomerService.Infrastructure \
  --startup-project src/CustomerService.Api \
  --context CustomerDbContext
```

Migrations existentes:

- `InitialCreateCustomers`
- `AddBankingDetails`
- `AddUsers`

## 19. Swagger/OpenAPI

Swagger é habilitado em ambiente `Development`.

URL local:

```text
http://localhost:5001/swagger
```

O Swagger inclui suporte a Bearer token e API versioning.

## 20. Decisões técnicas

- Separação em camadas seguindo Clean Architecture.
- `Domain` não depende de infraestrutura.
- `Application` define contratos e casos de uso.
- `Infrastructure` implementa persistência, cache, storage, token e hash de senha.
- `Api` concentra HTTP, autenticação, autorização, Swagger e middleware de erro.
- `User` é separado de `Customer`: usuário cuida de credenciais e role; cliente cuida de dados cadastrais/bancários.
- Saldo fica no `CustomerService` porque pertence aos dados bancários do cliente.
- Redis é tratado como otimização; PostgreSQL permanece como fonte da verdade.
- Blob Storage é abstraído para permitir fallback local sem credencial Azure.
- Testes de integração usam Testcontainers para reduzir dependência de infraestrutura manual.

## 21. Escopo atual e limitações

Implementado no código atual:

- API versionada em `/api/v1`.
- Health check.
- Login JWT.
- RBAC com roles `Admin`, `Customer` e `Service`.
- Seed de admin em desenvolvimento.
- Criação de cliente por Admin.
- Consulta de cliente autenticado.
- Consulta de cliente por Admin.
- Verificação de existência de cliente.
- Atualização parcial de cliente.
- Geração de URL temporária para upload de foto de perfil.
- Atualização de saldo entre dois clientes.
- PostgreSQL com EF Core/Npgsql.
- Redis para cache de existência e detalhes.
- Middleware global de exceções.
- Swagger com Bearer token.
- Dockerfile multi-stage.
- Docker Compose com API, PostgreSQL e Redis.
- Testes unitários e de integração.
- Application Insights opcional.

Não implementado no código atual:

- RabbitMQ ou qualquer broker de mensagens.
- Publicação do evento `CustomerBankingDetailsUpdated`.
- Upload binário da imagem pela API.
- Refresh token.
- Auditoria de alterações cadastrais.
- Rate limiting.
- Serilog.
- OpenTelemetry completo.
