# 1. IMAGEN DE CONSTRUCCIÓN (SDK)
# Usamos la imagen oficial de .NET 8 para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos el archivo de proyecto y descargamos las librerías
COPY ["FunerariaAPI.csproj", "./"]
RUN dotnet restore "FunerariaAPI.csproj"

# Copiamos todo lo demás (Carpetas, Controladores, Modelos)
COPY . .

# Construimos la versión final (Release)
RUN dotnet publish "FunerariaAPI.csproj" -c Release -o /app/publish

# 2. IMAGEN DE EJECUCIÓN (RUNTIME)
# Usamos una imagen más ligera solo para correr la app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .

# Comando de arranque (El nombre de la DLL debe coincidir con tu proyecto)
ENTRYPOINT ["dotnet", "FunerariaAPI.dll"]