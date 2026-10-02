# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["UnfollowersInstagram.csproj", "./"]
RUN dotnet restore "./UnfollowersInstagram.csproj"

COPY . .
RUN dotnet publish "UnfollowersInstagram.csproj" -c Release -o /app/publish

# 2. Etapa de producción (versión pineada con parches de seguridad)
FROM mcr.microsoft.com/dotnet/aspnet:10.0.12 AS base
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

RUN adduser --disabled-password --gecos "" --uid 10001 appuser && chown -R appuser:appuser /app
USER appuser

ENTRYPOINT ["dotnet", "UnfollowersInstagram.dll"]
