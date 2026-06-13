#!/usr/bin/env bash
# Shared helpers for deploy/recover scripts.

apply_nginx_domain() {
  local repo_dir="$1"
  local nginx_conf="$2"
  local site_domain="$3"

  if grep -q 'YOUR_DOMAIN' "$nginx_conf"; then
    sed -i "s/YOUR_DOMAIN/${site_domain}/g" "$nginx_conf"
  elif ! grep -Fq "$site_domain" "$nginx_conf"; then
    echo "[WARN]  Nginx config nie zawiera ${site_domain} — przywracam szablon."
    git -C "$repo_dir" checkout -- "$nginx_conf"
    sed -i "s/YOUR_DOMAIN/${site_domain}/g" "$nginx_conf"
  fi

  if grep -q 'YOUR_DOMAIN' "$nginx_conf"; then
    echo "[ERR]   Nginx nadal zawiera YOUR_DOMAIN — sprawdź deploy/.env i plik $nginx_conf"
    return 1
  fi
}

wait_for_compose_health() {
  local compose_file="$1"
  local env_file="$2"
  local service="$3"
  local max_retries="${4:-60}"

  local retries=0
  while true; do
    local cid status
    cid="$(docker compose -f "$compose_file" --env-file "$env_file" ps -q "$service" 2>/dev/null | head -1)"
    if [[ -z "$cid" ]]; then
      retries=$((retries + 1))
      [[ $retries -ge $max_retries ]] && return 1
      sleep 2
      continue
    fi

    status="$(docker inspect -f '{{if .State.Health}}{{.State.Health.Status}}{{else}}healthy{{end}}' "$cid" 2>/dev/null || echo unknown)"
    if [[ "$status" == "healthy" ]]; then
      return 0
    fi

    retries=$((retries + 1))
    if [[ $retries -ge $max_retries ]]; then
      return 1
    fi
    sleep 2
  done
}

reload_nginx() {
  local compose_file="$1"
  local env_file="$2"

  if ! docker compose -f "$compose_file" --env-file "$env_file" ps -q nginx | grep -q .; then
    docker compose -f "$compose_file" --env-file "$env_file" up -d nginx
    return 0
  fi

  docker compose -f "$compose_file" --env-file "$env_file" exec -T nginx nginx -t
  docker compose -f "$compose_file" --env-file "$env_file" exec -T nginx nginx -s reload
}
