#!/usr/bin/env bash
# Backup produkcyjnej bazy PostgreSQL (kontener typer-postgres).
# Użycie na VPS: bash scripts/backup-db.sh
set -euo pipefail

REPO_DIR="$(cd "$(dirname "$0")/.." && pwd)"
ENV_FILE="$REPO_DIR/deploy/.env"
BACKUP_DIR="${BACKUP_DIR:-/opt/typer-backups}"
CONTAINER="${POSTGRES_CONTAINER:-typer-postgres}"

[[ -f "$ENV_FILE" ]] || { echo "Brak pliku $ENV_FILE"; exit 1; }

DB_USER="$(grep -E '^DB_USER=' "$ENV_FILE" | cut -d= -f2-)"
DB_NAME="$(grep -E '^DB_NAME=' "$ENV_FILE" | cut -d= -f2-)"

[[ -n "$DB_USER" && -n "$DB_NAME" ]] || { echo "Ustaw DB_USER i DB_NAME w deploy/.env"; exit 1; }

mkdir -p "$BACKUP_DIR"

STAMP="$(date +%Y%m%d_%H%M%S)"
OUT="$BACKUP_DIR/typer_${DB_NAME}_${STAMP}.sql.gz"

docker exec "$CONTAINER" pg_dump -U "$DB_USER" -d "$DB_NAME" --no-owner --no-acl | gzip > "$OUT"

echo "Backup zapisany: $OUT"
ls -lh "$OUT"
