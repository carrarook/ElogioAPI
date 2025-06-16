# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia TODOS os arquivos do projeto para o contêiner
# Como o contexto do build é a pasta "ElogioAPI", isso copiará tudo corretamente.
COPY . .

# Restaura as dependências e publica a aplicação em modo de Release
RUN dotnet publish "ElogioAPI.csproj" -c Release -o /app/publish

# Estágio 2: Imagem Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ElogioAPI.dll"]
