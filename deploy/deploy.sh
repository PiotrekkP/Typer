#!/usr/bin/env bash
# =============================================================
# Typer — deployment script for Ubuntu 24.04
# Usage: bash deploy.sh [--skip-pull] [--skip-migrate]
# =============================================================
set -euo pipefail

REPO_DIR="$(cd "$(dirname "$0")/.." && pwd)"
COMPOSE_FILE="$REPO_DIR/docker-compose.prod.yml"
ENV_FILE="$REPO_DIR/deploy/.env"
NGINX_CONF="$REPO_DIR/deploy/nginx/conf.d/typer.conf"

SKIP_PULL=false
SKIP_MIGRATE=false
FULL_BUILD=false

for arg in "$@"; do
  case $arg in
    --skip-pull)    SKIP_PULL=true ;;
    --skip-migrate) SKIP_MIGRATE=true ;;
    --full-build)   FULL_BUILD=true ;;
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
  if ! git -C "$REPO_DIR" diff --quiet -- "$NGINX_CONF" 2>/dev/null; then
    warning "Local nginx config differs from git — resetting before pull (domain re-applied from deploy/.env)."
    git -C "$REPO_DIR" checkout -- "$NGINX_CONF"
  fi
  git -C "$REPO_DIR" pull --ff-only
fi

# ── Nginx domain (from deploy/.env) ───────────────────────────
set -a
# shellcheck source=/dev/null
source "$ENV_FILE"
set +a
SITE_DOMAIN="${SITE_DOMAIN:-}"
if [[ -n "$SITE_DOMAIN" ]]; then
  info "Applying nginx domain: $SITE_DOMAIN"
  if grep -q 'YOUR_DOMAIN' "$NGINX_CONF"; then
    sed -i "s/YOUR_DOMAIN/${SITE_DOMAIN}/g" "$NGINX_CONF"
  else
    info "Nginx config already has domain applied — skipping sed."
  fi
else
  warning "SITE_DOMAIN not set in deploy/.env — nginx still uses YOUR_DOMAIN placeholder."
fi

# ── Build images ──────────────────────────────────────────────
BUILD_VERSION="$(git -C "$REPO_DIR" rev-parse --short HEAD 2>/dev/null || date -u +%Y%m%d%H%M)"
export BUILD_VERSION
info "Build version (asset cache bust): $BUILD_VERSION"

info "Building Docker images..."
BUILD_ARGS=()
[[ "$FULL_BUILD" == true ]] && BUILD_ARGS+=(--no-cache)
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" build "${BUILD_ARGS[@]}"

# ── Start / recreate containers ───────────────────────────────
info "Starting PostgreSQL..."
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d postgres

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

# ── Run EF Core migrations (before api/web — new schema required) ─
if [[ "$SKIP_MIGRATE" == false ]]; then
  info "Applying EF Core migrations..."
  DB_NAME="$(grep -E '^DB_NAME=' "$ENV_FILE" | cut -d= -f2-)"
  DB_USER="$(grep -E '^DB_USER=' "$ENV_FILE" | cut -d= -f2-)"
  DB_PASSWORD="$(grep -E '^DB_PASSWORD=' "$ENV_FILE" | cut -d= -f2-)"
  NETWORK="$(docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" ps -q postgres 2>/dev/null \
    | head -1 | xargs -r docker inspect -f '{{range $k, $v := .NetworkSettings.Networks}}{{$k}}{{end}}' \
    | head -1)"

  if [[ -z "$NETWORK" ]]; then
    NETWORK="${COMPOSE_PROJECT_NAME:-typer}_typer-internal"
  fi

  docker run --rm \
    --network "$NETWORK" \
    -v "$REPO_DIR:/app" \
    -w /app \
    -e "ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}" \
    mcr.microsoft.com/dotnet/sdk:8.0 \
    bash -c "dotnet tool restore && dotnet ef database update --project src/Typer.Infrastructure --startup-project src/Typer.Api" \
    || error "Migration failed — aborting deploy to protect the database."
fi

info "Starting all services..."
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --remove-orphans

# ── Health check ──────────────────────────────────────────────
info "Checking service health..."
sleep 5
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" ps

info "Deployment complete! Services:"
echo "  Web UI  →  https://${SITE_DOMAIN:-YOUR_DOMAIN}"
echo "  API     →  https://${SITE_DOMAIN:-YOUR_DOMAIN}/api"
echo "  Swagger →  https://${SITE_DOMAIN:-YOUR_DOMAIN}/swagger"
