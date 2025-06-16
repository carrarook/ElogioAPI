# Estágio 1: Build
# Usa a imagem completa do SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia os arquivos de projeto (.csproj e .sln)
COPY ["ElogioAPI.sln", "."]
COPY ["ElogioAPI/ElogioAPI.csproj", "ElogioAPI/"]

# Restaura as dependências de todo o projeto
RUN dotnet restore "ElogioAPI.sln"

# Copia todo o resto do código fonte
COPY . .
WORKDIR "/src/ElogioAPI"

# Publica a aplicação, otimizada para produção
RUN dotnet publish "ElogioAPI.csproj" -c Release -o /app/publish

# Estágio 2: Imagem Final
# Usa a imagem leve do ASP.NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ElogioAPI.dll"]