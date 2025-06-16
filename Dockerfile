# Estágio 1: Restaurar dependências
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

# Copia o arquivo .csproj, que está dentro da pasta ElogioAPI, para uma pasta com o mesmo nome no contêiner
COPY ["ElogioAPI/ElogioAPI.csproj", "ElogioAPI/"]

# Restaura os pacotes NuGet, especificando o caminho completo para o arquivo de projeto
RUN dotnet restore "ElogioAPI/ElogioAPI.csproj"

# Estágio 2: Compilar e Publicar
FROM restore AS publish
WORKDIR /src

# Copia todo o código-fonte da pasta ElogioAPI para a pasta correspondente no contêiner
COPY ["ElogioAPI/", "ElogioAPI/"]

# Publica a aplicação, especificando o caminho completo para o arquivo de projeto
RUN dotnet publish "ElogioAPI/ElogioAPI.csproj" -c Release -o /app/publish --no-restore

# Estágio 3: Imagem Final (Runtime)
# Usa a imagem leve do ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ElogioAPI.dll"]