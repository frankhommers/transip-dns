ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY src/TransIp.Dns.Cli/TransIp.Dns.Cli.csproj src/TransIp.Dns.Cli/
RUN dotnet restore src/TransIp.Dns.Cli/TransIp.Dns.Cli.csproj

COPY src/ src/
RUN dotnet publish src/TransIp.Dns.Cli/TransIp.Dns.Cli.csproj \
      -c Release \
      --no-restore \
      -o /app

FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION} AS runtime
WORKDIR /app
COPY --from=build /app .

LABEL org.opencontainers.image.source="https://github.com/frankhommers/transip-dns" \
      org.opencontainers.image.description="TransIP DNS record CRUD CLI." \
      org.opencontainers.image.licenses="MIT"

ENTRYPOINT ["dotnet", "/app/TransIp.Dns.Cli.dll"]
