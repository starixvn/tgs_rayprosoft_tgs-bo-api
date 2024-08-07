FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["BackendOffice/BackendOffice.csproj", "BackendOffice/"]
COPY ["BackOffice.Core.Cache/BackOffice.Core.Cache.csproj", "BackOffice.Core.Cache/"]
COPY ["BackOffice/BackOffice.csproj", "BackOffice/"]
COPY ["Bucket/PlayerBucket.Core.csproj", "Bucket/"]
COPY ["Wallet.Core/Wallet.Core.csproj", "Wallet.Core/"]
COPY ["BackOffice.Repositories/BackOffice.Repositories.csproj", "BackOffice.Repositories/"]
COPY ["PlayerBucket.Repositories/PlayerBucket.Repositories.csproj", "PlayerBucket.Repositories/"]
COPY ["Wallet.Repositories/Wallet.Repositories.csproj", "Wallet.Repositories/"]
RUN dotnet restore "./BackendOffice/./BackendOffice.csproj"
COPY . .
WORKDIR "/src/BackendOffice"
RUN dotnet build "./BackendOffice.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BackendOffice.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
COPY --from=publish /app/publish .
COPY fonts /usr/share/fonts/
ENTRYPOINT ["dotnet", "TGS.Apis.BackendOffice.dll"]