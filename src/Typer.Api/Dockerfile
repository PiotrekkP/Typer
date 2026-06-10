# ── Build stage ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore (layer-cached)
COPY src/Typer.Domain/Typer.Domain.csproj                       src/Typer.Domain/
COPY src/Typer.Application/Typer.Application.csproj             src/Typer.Application/
COPY src/Typer.Infrastructure/Typer.Infrastructure.csproj        src/Typer.Infrastructure/
COPY src/Typer.Api/Typer.Api.csproj                              src/Typer.Api/

RUN dotnet restore src/Typer.Api/Typer.Api.csproj

# Copy source and publish
COPY src/ src/
ARG BUILD_VERSION=dev
RUN dotnet publish src/Typer.Api/Typer.Api.csproj \
    -c Release \
    -o /app/publish \
    -p:InformationalVersion=${BUILD_VERSION} \
    --no-restore

# ── Runtime stage ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup --system typer && adduser --system --ingroup typer typer

COPY --from=build /app/publish .
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && chown -R typer:typer /app

USER typer

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "Typer.Api.dll"]
