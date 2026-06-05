#!/usr/bin/env bash
# =============================================================
# Typer — deployment script for Ubuntu 24.04
# Usage: bash deploy.sh [--skip-pull] [--skip-migrate]
# =============================================================
set -euo pipefail

REPO_DIR="$(cd "$(dirname "$0")/.." && pwd)"
COMPOSE_FILE="$REPO_DIR/docker-compose.prod.yml"
ENV_FILE="$REPO_DIR/deploy/.env"

SKIP_PULL=false
SKIP_MIGRATE=false

for arg in "$@"; do
  case $arg in
    --skip-pull)    SKIP_PULL=true ;;
    --skip-migrate) SKIP_MIGRATE=true ;;
  esac
done

# ── Colour output ─────────────────────────────────────────────
GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
info()    { echo -e "${GREEN}[INFO]${NC}  $*"; }
warning() { echo -e "${YELLOW}[WARN]${NC}  $*"; }
error()   { echo -e "${RED}[ERR]${NC}   $*"; exit 1; }

# ── Checks ────────────────────────────────────────────────────
[[ -f "$ENV_FILE" ]] || error ".env file not found at $ENV_FILE — copy deploy/.env.example and fill values."
command -v docker &>/dev/null || error "Docker is not installed."
docker compose version &>/dev/null || error "Docker Compose plugin not installed."

info "Working directory: $REPO_DIR"

# ── Pull latest code ──────────────────────────────────────────
if [[ "$SKIP_PULL" == false ]]; then
  info "Pulling latest code from git..."
  git -C "$REPO_DIR" pull --ff-only
fi

# ── Build images ──────────────────────────────────────────────
info "Building Docker images..."
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" build --no-cache

# ── Start / recreate containers ───────────────────────────────
info "Starting services..."
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --remove-orphans

# ── Wait for PostgreSQL ───────────────────────────────────────
info "Waiting for PostgreSQL to be healthy..."
RETRIES=0
until docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" \
        exec -T postgres pg_isready -U "$(grep DB_USER "$ENV_FILE" | cut -d= -f2)" &>/dev/null; do
  RETRIES=$((RETRIES+1))
  [[ $RETRIES -ge 20 ]] && error "PostgreSQL did not become healthy in time."
  sleep 3
done
info "PostgreSQL is ready."

# ── Run EF Core migrations ────────────────────────────────────
if [[ "$SKIP_MIGRATE" == false ]]; then
  info "Applying EF Core migrations..."
  docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" \
    exec -T api dotnet Typer.Api.dll migrate 2>/dev/null || \
  docker run --rm \
    --network "$(docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" \
      ps -q postgres | xargs docker inspect -f '{{range .NetworkSettings.Networks}}{{.NetworkID}}{{end}}' | head -1)" \
    --env-file "$ENV_FILE" \
    -e "ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=$(grep DB_NAME "$ENV_FILE" | cut -d= -f2);Username=$(grep DB_USER "$ENV_FILE" | cut -d= -f2);Password=$(grep DB_PASSWORD "$ENV_FILE" | cut -d= -f2)" \
    mcr.microsoft.com/dotnet/sdk:8.0 bash -c "
      apt-get update -qq && apt-get install -y -qq git &&
      git clone https://github.com/YOUR_USERNAME/Typer /tmp/typer &&
      cd /tmp/typer &&
      dotnet tool restore &&
      dotnet ef database update --project src/Typer.Infrastructure --startup-project src/Typer.Api
    " || warning "Migration step skipped — run manually if needed."
fi

# ── Health check ──────────────────────────────────────────────
info "Checking service health..."
sleep 5
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" ps

info "Deployment complete! Services:"
echo "  Web UI  →  https://YOUR_DOMAIN"
echo "  API     →  https://YOUR_DOMAIN/api"
echo "  Swagger →  https://YOUR_DOMAIN/swagger"
