# Base image pro runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Expose HTTPS port
EXPOSE 443

# Pokud potřebuješ i HTTP (např. pro redirect), můžeš přidat
# EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["server/server.csproj", "server/"]
RUN dotnet restore "./server/server.csproj"
COPY . .
WORKDIR "/src/server"
RUN dotnet build "./server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Připoj certifikáty do kontejneru
# Předpokládáme, že certs jsou mapované přes volume z docker-compose
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/api.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=mypassword

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "server.dll"]
