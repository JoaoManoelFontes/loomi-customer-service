FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/CustomerService.Api/CustomerService.Api.csproj", "src/CustomerService.Api/"]
COPY ["src/CustomerService.Application/CustomerService.Application.csproj", "src/CustomerService.Application/"]
COPY ["src/CustomerService.Domain/CustomerService.Domain.csproj", "src/CustomerService.Domain/"]
COPY ["src/CustomerService.Infrastructure/CustomerService.Infrastructure.csproj", "src/CustomerService.Infrastructure/"]

RUN dotnet restore "src/CustomerService.Api/CustomerService.Api.csproj"

COPY . .
RUN dotnet publish "src/CustomerService.Api/CustomerService.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CustomerService.Api.dll"]
