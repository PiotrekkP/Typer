#!/usr/bin/env bash
# Szybkie podniesienie serwisu po przerwanym deployu (bez pełnego rebuildu).
# Usage: bash deploy/recover.sh
set -euo pipefail

REPO_DIR="$(cd "$(dirname "$0")/.." && pwd)"
COMPOSE_FILE="$REPO_DIR/docker-compose.prod.yml"
ENV_FILE="$REPO_DIR/deploy/.env"
NGINX_CONF="$REPO_DIR/deploy/nginx/conf.d/typer.conf"
# shellcheck source=lib.sh
source "$(dirname "$0")/lib.sh"

GREEN='\033[0;32m'; RED='\033[0;31m'; NC='\033[0m'
info() { echo -e "${GREEN}[INFO]${NC}  $*"; }
error() { echo -e "${RED}[ERR]${NC}   $*"; exit 1; }

[[ -f "$ENV_FILE" ]] || error "Brak $ENV_FILE"

set -a
# shellcheck source=/dev/null
source "$ENV_FILE"
set +a

SITE_DOMAIN="${SITE_DOMAIN:-}"
if [[ -z "$SITE_DOMAIN" ]]; then
  error "Brak SITE_DOMAIN w deploy/.env — dodaj np. SITE_DOMAIN=cozatypy.pl"
fi

if grep -q 'YOUR_DOMAIN' "$NGINX_CONF"; then
  info "Ustawiam domenę nginx: $SITE_DOMAIN"
  apply_nginx_domain "$REPO_DIR" "$NGINX_CONF" "$SITE_DOMAIN"
elif ! grep -Fq "$SITE_DOMAIN" "$NGINX_CONF"; then
  info "Ustawiam domenę nginx: $SITE_DOMAIN"
  apply_nginx_domain "$REPO_DIR" "$NGINX_CONF" "$SITE_DOMAIN"
fi

info "PostgreSQL..."
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d postgres

RETRIES=0
until docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" \
        exec -T postgres pg_isready -U "${DB_USER}" &>/dev/null; do
  RETRIES=$((RETRIES+1))
  [[ $RETRIES -ge 20 ]] && error "PostgreSQL nie odpowiada."
  sleep 2
done

info "Migracje..."
DB_NAME="${DB_NAME}"
DB_USER="${DB_USER}"
DB_PASSWORD="${DB_PASSWORD}"
NETWORK="$(docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" ps -q postgres \
  | head -1 | xargs -r docker inspect -f '{{range $k, $v := .NetworkSettings.Networks}}{{$k}}{{end}}' \
  | head -1)"
[[ -z "$NETWORK" ]] && NETWORK="${COMPOSE_PROJECT_NAME:-typer}_typer-internal"

docker run --rm \
  --network "$NETWORK" \
  -v "$REPO_DIR:/app" \
  -w /app \
  -e "ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}" \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  bash -c "dotnet tool restore && dotnet ef database update --project src/Typer.Infrastructure --startup-project src/Typer.Api"

info "Start wszystkich kontenerów..."
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --remove-orphans

info "Waiting for API to be healthy..."
wait_for_compose_health "$COMPOSE_FILE" "$ENV_FILE" api 60 || true

info "Waiting for Web to be healthy..."
wait_for_compose_health "$COMPOSE_FILE" "$ENV_FILE" web 90 || true

info "Reloading nginx..."
reload_nginx "$COMPOSE_FILE" "$ENV_FILE" || docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" restart nginx

sleep 5
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" ps

info "Sprawdź: curl -fs http://localhost/health && echo OK"
