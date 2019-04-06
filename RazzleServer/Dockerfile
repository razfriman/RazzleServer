# Build
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 as build
WORKDIR /src
COPY RazzleServer.sln ./
COPY RazzleServer/RazzleServer.csproj RazzleServer/
COPY RazzleServer.DB/RazzleServer.Data.csproj RazzleServer.DB/
COPY RazzleServer.Common/RazzleServer.Common.csproj RazzleServer.Common/
COPY RazzleServer.Crypto/RazzleServer.Crypto.csproj RazzleServer.Crypto/
COPY RazzleServer.Net/RazzleServer.Net.csproj RazzleServer.Net/
COPY RazzleServer.Wz/RazzleServer.Wz.csproj RazzleServer.Wz/
COPY RazzleServer.Tests/RazzleServer.Tests.csproj RazzleServer.Tests/
RUN dotnet restore
COPY . .

# Publish
FROM build AS publish
WORKDIR /src/RazzleServer
RUN dotnet publish -c Release -o /app

# Run
FROM mcr.microsoft.com/dotnet/core/runtime:3.0 as runtime
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RazzleServer.dll"]