FROM microsoft/dotnet:2.1-runtime-alpine AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /src
COPY RazzleServer.sln ./
COPY RazzleServer/RazzleServer.csproj RazzleServer/
COPY RazzleServer.DB/RazzleServer.Data.csproj RazzleServer.DB/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/RazzleServer
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RazzleServer.dll"]
