# Usar la imagen base de .NET 8 SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de solución y proyectos
COPY ["LicoreriaAPI.sln", "./"]
COPY ["src/LicoreriaAPI.Domain/LicoreriaAPI.Domain.csproj", "src/LicoreriaAPI.Domain/"]
COPY ["src/LicoreriaAPI.DTOs/LicoreriaAPI.DTOs.csproj", "src/LicoreriaAPI.DTOs/"]
COPY ["src/LicoreriaAPI.Infrastructure/LicoreriaAPI.Infrastructure.csproj", "src/LicoreriaAPI.Infrastructure/"]
COPY ["src/LicoreriaAPI.Application/LicoreriaAPI.Application.csproj", "src/LicoreriaAPI.Application/"]
COPY ["src/LicoreriaAPI/LicoreriaAPI.csproj", "src/LicoreriaAPI/"]

# Restaurar dependencias
RUN dotnet restore "LicoreriaAPI.sln"

# Copiar todo el código fuente
COPY . .

# Compilar y publicar la aplicación
WORKDIR "/src/src/LicoreriaAPI"
RUN dotnet build "LicoreriaAPI.csproj" -c Release -o /app/build
RUN dotnet publish "LicoreriaAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final más liviana con runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Crear usuario no-root para seguridad
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copiar archivos publicados
COPY --from=build /app/publish .

# Configurar permisos
RUN chown -R appuser:appuser /app
USER appuser

# Exponer el puerto 8080 (Azure App Service usa este puerto por defecto)
EXPOSE 8080

# Variables de entorno para Azure
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Punto de entrada
ENTRYPOINT ["dotnet", "LicoreriaAPI.dll"]


