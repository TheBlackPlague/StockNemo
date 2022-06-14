FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /StockNemo

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Terminal/Terminal.csproj", "Terminal/"]
COPY ["Backend/Backend.csproj", "Backend/"]
RUN dotnet restore "Terminal/Terminal.csproj"
COPY . .
WORKDIR "/src/Terminal"
RUN dotnet build "Terminal.csproj" -c Release -o /StockNemo/build

FROM build AS publish
RUN dotnet publish "Terminal.csproj" -c Release -o /App
WORKDIR /App

LABEL org.opencontainers.image.source=https://github.com/TheBlackPlague/StockNemo

#FROM base AS final
#WORKDIR /StockNemo
#COPY --from=publish /StockNemo/publish .
#ENTRYPOINT ["dotnet", "StockNemo.dll"]
