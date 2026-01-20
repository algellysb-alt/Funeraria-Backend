# 1. IMAGEN DE CONSTRUCCIÓN (SDK)
# Actualizado a 10.0 para que coincida con tu proyecto
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos el archivo de proyecto y descargamos las librerías
COPY ["FunerariaAPI.csproj", "./"]
RUN dotnet restore "FunerariaAPI.csproj"

# Copiamos todo lo demás (Carpetas, Controladores, Modelos)
COPY . .

# Construimos la versión final (Release)
RUN dotnet publish "FunerariaAPI.csproj" -c Release -o /app/publish

# 2. IMAGEN DE EJECUCIÓN (RUNTIME)
# Actualizado a 10.0 para poder correr la app
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .

# Comando de arranque
ENTRYPOINT ["dotnet", "FunerariaAPI.dll"]
