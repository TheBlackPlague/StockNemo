FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base

LABEL org.opencontainers.image.source=https://github.com/TheBlackPlague/StockNemo

WORKDIR /StockNemo

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Terminal/Terminal.csproj", "Terminal/"]
COPY ["Backend/Backend.csproj", "Backend/"]
RUN dotnet restore "Terminal/Terminal.csproj"
COPY . .
WORKDIR "/src/Terminal"
RUN dotnet publish "Terminal.csproj" -c Release -r linux-x64 -o /App  \
    /p:PublishSingleFile=true --self-contained true
WORKDIR /App
