FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# On utilise le port 10000 (standard pour Render)
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# On copie tout le contenu pour être sûr de ne rien rater
COPY . .

# On restaure les dépendances
RUN dotnet restore "GestionVentesAPI.csproj"

# On compile et on publie
RUN dotnet publish "GestionVentesAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "GestionVentesAPI.dll"]