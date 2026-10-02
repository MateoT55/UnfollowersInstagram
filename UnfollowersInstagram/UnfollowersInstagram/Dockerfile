# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["UnfollowersInstagram.csproj", "./"]
RUN dotnet restore "./UnfollowersInstagram.csproj"

COPY . .
RUN dotnet publish "UnfollowersInstagram.csproj" -c Release -o /app/publish

# 2. Etapa de producción
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UnfollowersInstagram.dll"]