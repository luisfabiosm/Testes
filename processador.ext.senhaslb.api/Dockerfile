#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/processador.ext.senhaslb.api/processador.ext.senhaslb.api.csproj", "src/processador.ext.senhaslb.api/"]
COPY ["src/w3socket/w3socket.csproj", "src/w3socket/"]
RUN dotnet restore "./src/processador.ext.senhaslb.api/processador.ext.senhaslb.api.csproj"
COPY . .
WORKDIR "/src/src/processador.ext.senhaslb.api"
RUN dotnet build "./processador.ext.senhaslb.api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./processador.ext.senhaslb.api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "processador-ext-senhaslb-api.dll"]