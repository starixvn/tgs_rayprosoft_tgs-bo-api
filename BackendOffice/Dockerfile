#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
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
RUN dotnet restore "BackendOffice/BackendOffice.csproj"
COPY . .
WORKDIR "/src/BackendOffice"
RUN dotnet build "BackendOffice.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "BackendOffice.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY fonts /usr/share/
ENTRYPOINT ["dotnet", "TGS.Apis.BackendOffice.dll"]