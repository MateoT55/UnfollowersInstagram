# ==========================================
# ETAPA 1: Construcción (SDK .NET 10)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos solo el archivo del proyecto y restauramos las dependencias
COPY ["UnfollowersInstagram.csproj", "./"]
RUN dotnet restore "./UnfollowersInstagram.csproj"

# Copiamos el resto del código
COPY . .

# Compilamos en modo Release
RUN dotnet publish "UnfollowersInstagram.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# ETAPA 2: Producción (Runtime .NET 10 ligero)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Usamos el usuario 'app' que Microsoft incluye sin permisos de root
USER app

# .NET escucha por defecto en el puerto 8080
EXPOSE 8080

# Traemos los archivos ya compilados desde la Etapa 1
COPY --from=build /app/publish .

# Arrancamos la aplicación
ENTRYPOINT ["dotnet", "UnfollowersInstagram.dll"]