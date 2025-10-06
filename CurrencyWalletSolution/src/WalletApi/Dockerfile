FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["WalletApi/WalletApi.csproj", "WalletApi/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["ECBGateway/ECBGateway.csproj", "ECBGateway/"]
COPY ["CurrencyUpdater/CurrencyUpdater.csproj", "CurrencyUpdater/"]

# Restore
RUN dotnet restore "WalletApi/WalletApi.csproj"

# Copy all source code
COPY . .

# Remove conflicting appsettings from CurrencyUpdater
RUN rm -f /src/CurrencyUpdater/appsettings.json && \
    rm -f /src/CurrencyUpdater/appsettings.Development.json

# Build
WORKDIR "/src/WalletApi"
RUN dotnet build "WalletApi.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "WalletApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WalletApi.dll"]