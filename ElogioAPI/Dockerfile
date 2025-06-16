# Estágio 1: Build da Aplicação
# Usamos a imagem completa do SDK do .NET 9 para compilar o projeto.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia os arquivos .csproj e restaura as dependências primeiro
# Isso aproveita o cache do Docker se as dependências não mudarem.
COPY ["ElogioAPI.csproj", "./"]
RUN dotnet restore "./ElogioAPI.csproj"

# Copia todo o resto do código-fonte e compila
COPY . .
WORKDIR "/src/."
RUN dotnet build "ElogioAPI.csproj" -c Release -o /app/build

# Publica a aplicação, gerando os artefatos otimizados para produção
FROM build AS publish
RUN dotnet publish "ElogioAPI.csproj" -c Release -o /app/publish

# Estágio 2: Imagem Final
# Usamos a imagem leve do ASP.NET Runtime, que é menor e mais segura.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Define o ponto de entrada para a aplicação
ENTRYPOINT ["dotnet", "ElogioAPI.dll"]