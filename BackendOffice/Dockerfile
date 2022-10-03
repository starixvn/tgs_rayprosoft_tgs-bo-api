#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/Apis/BackendOffice/BackendOffice.csproj", "src/Apis/BackendOffice/"]
COPY ["src/Repositories/BackOffice.Repositories/BackOffice.Repositories.csproj", "src/Repositories/BackOffice.Repositories/"]
COPY ["src/Services/BackOffice/BackOffice.csproj", "src/Services/BackOffice/"]
COPY ["src/Repositories/TgsTransaction.Repositories/TgsTransaction.Repositories.csproj", "src/Repositories/TgsTransaction.Repositories/"]
COPY ["src/Services/TgsTransaction.Core/TgsTransaction.Core.csproj", "src/Services/TgsTransaction.Core/"]
COPY ["src/Services/Bucket/PlayerBucket.Core.csproj", "src/Services/Bucket/"]
COPY ["src/Services/Wallet.Core/Wallet.Core.csproj", "src/Services/Wallet.Core/"]
COPY ["src/Repositories/PlayerBucket.Repositories/PlayerBucket.Repositories.csproj", "src/Repositories/PlayerBucket.Repositories/"]
COPY ["src/Repositories/Wallet.Repositories/Wallet.Repositories.csproj", "src/Repositories/Wallet.Repositories/"]
COPY ["src/Services/BackOffice.Core.Cache/BackOffice.Core.Cache.csproj", "src/Services/BackOffice.Core.Cache/"]
COPY ["src/Services/Operations/Operations.csproj", "src/Services/Operations/"]
RUN dotnet restore "src/Apis/BackendOffice/BackendOffice.csproj"
COPY . .
WORKDIR "/src/src/Apis/BackendOffice"
RUN dotnet build "BackendOffice.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "BackendOffice.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TGS.Apis.BackendOffice.dll"]