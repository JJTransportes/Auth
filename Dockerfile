FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER app
WORKDIR /app
EXPOSE 5100

# O contexto de build é a raiz do PepiX (ver docker-compose.yaml),
# para que o projeto Contracts fique acessível.
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Contracts/Contracts.csproj", "Contracts/"]
COPY ["Auth/Auth.csproj", "Auth/"]
RUN dotnet restore "Auth/Auth.csproj"
COPY ["Contracts/", "Contracts/"]
COPY ["Auth/", "Auth/"]
WORKDIR "/src/Auth"
RUN dotnet build "./Auth.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Auth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.dll"]
