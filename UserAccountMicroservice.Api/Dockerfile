FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copiar los .csproj de TODOS los proyectos involucrados
# (El de la API y el de la Librería de la que depende)
# Nota las rutas: "Carpeta/Archivo.csproj"
COPY ["UserAccountMicroservice.Api/UserAccountMicroservice.Api.csproj", "UserAccountMicroservice.Api/"]
COPY ["UserAccountMicroservice.Application/UserAccountMicroservice.Application.csproj", "UserAccountMicroservice.Application/"]
COPY ["UserAccountMicroservice.Domain/UserAccountMicroservice.Domain.csproj", "UserAccountMicroservice.Domain/"]
COPY ["UserAccountMicroservice.Infrastructure/UserAccountMicroservice.Infrastructure.csproj", "UserAccountMicroservice.Infrastructure/"]

# 2. Restaurar dependencias (NuGet)
# Al restaurar el principal, .NET buscará las referencias
RUN dotnet restore "UserAccountMicroservice.Api/UserAccountMicroservice.Api.csproj"

# 3. Copiar TODO el resto del código fuente
# Esto copia /MiSolucion (contexto) a /src (contenedor)
COPY . .

# 4. Cambiar al directorio del proyecto principal para compilar
WORKDIR "/src/UserAccountMicroservice.Api"
RUN dotnet build "UserAccountMicroservice.Api.csproj" -c Release -o /app/build

# 5. Publicar
FROM build AS publish
RUN dotnet publish "UserAccountMicroservice.Api.csproj" -c Release -o /app/publish

# 6. Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserAccountMicroservice.Api.dll"]
