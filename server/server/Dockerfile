# Base image pro runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Expose HTTP port pro vývoj
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY server.csproj ./
RUN dotnet restore "server.csproj"
COPY . .
RUN dotnet build "server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Nastavení pro HTTP (bez certifikátů)
ENV ASPNETCORE_URLS=http://+:80

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "server.dll"]