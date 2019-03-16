FROM microsoft/dotnet:3.0-runtime-alpine AS base
WORKDIR /app

FROM microsoft/dotnet:3.0-sdk-alpine AS build
WORKDIR /src
COPY RazzleServer.sln ./
COPY RazzleServer/RazzleServer.csproj RazzleServer/
COPY RazzleServer.DB/RazzleServer.Data.csproj RazzleServer.DB/
COPY RazzleServer.Common/RazzleServer.Common.csproj RazzleServer.Common/
COPY RazzleServer.Tests/RazzleServer.Tests.csproj RazzleServer.Tests/
RUN dotnet restore
COPY . .

WORKDIR /src/RazzleServer
RUN dotnet build -c Release -o /app

WORKDIR /src/RazzleServer.Tests
RUN dotnet test -c Release

WORKDIR /src/RazzleServer
FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RazzleServer.dll"]