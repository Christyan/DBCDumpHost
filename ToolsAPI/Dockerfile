#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["ToolsAPI/ToolsAPI.csproj", "ToolsAPI/"]
COPY ["DBCD/DBFileReaderLib/DBFileReaderLib.csproj", "DBCD/DBFileReaderLib/"]
COPY ["DBDiffer/DBDiffer/DBDiffer.csproj", "DBDiffer/DBDiffer/"]
COPY ["WoWTools.SpellDescParser/WoWTools.SpellDescParser.csproj", "WoWTools.SpellDescParser/"]
COPY ["DBCD/DBCD/DBCD.csproj", "DBCD/DBCD/"]
RUN dotnet restore "ToolsAPI/ToolsAPI.csproj"
COPY . .
WORKDIR "/src/ToolsAPI"
RUN dotnet build "ToolsAPI.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "ToolsAPI.csproj" -c Release -o /app/publish
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToolsAPI.dll"]