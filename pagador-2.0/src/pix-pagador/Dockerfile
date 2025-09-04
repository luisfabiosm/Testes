# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Instalar ICU para suporte completo de globalização
#RUN apk add --no-cache icu-libs

RUN sed -i 's/\[openssl_init\]/# [openssl_init]/' /etc/ssl/openssl.cnf
RUN printf "\n\n[openssl_init]\nssl_conf = ssl_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_sect]\nsystem_default = ssl_default_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_default_sect]\nMinProtocol = TLSv1\nCipherString = DEFAULT@SECLEVEL=0\n" >> /etc/ssl/openssl.cnf

EXPOSE 8080
EXPOSE 8081


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/pix-pagador/pix-pagador.csproj", "src/pix-pagador/"]
RUN dotnet restore "./src/pix-pagador/pix-pagador.csproj"
COPY . .
WORKDIR "/src/src/pix-pagador"
RUN dotnet build "./pix-pagador.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./pix-pagador.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

RUN find /app/publish -name "*.pdb" -delete && \
    find /app/publish -name "*.xml" -delete && \
    find /app/publish -name "*.deps.json" -delete && \
    rm -rf /app/publish/refs && \
    rm -rf /app/publish/runtimes/win* && \
    rm -rf /app/publish/runtimes/osx*

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pagador.WebAPI.dll"]