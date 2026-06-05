# Typer — Deployment na Ubuntu 24.04

Poniższa instrukcja przeprowadza przez pełne wdrożenie na VPS z Ubuntu 24.04.

## Architektura

```
Internet (80/443)
       │
   [Nginx]  ← TLS termination, reverse proxy
    │    │
  [Web] [API]   ← kontenery Docker
         │
     [PostgreSQL]
```

---

## 1. Przygotowanie serwera

### 1.1 Aktualizacja systemu

```bash
sudo apt update && sudo apt upgrade -y
sudo apt install -y git curl ufw
```

### 1.2 Firewall

```bash
sudo ufw allow OpenSSH
sudo ufw allow 80
sudo ufw allow 443
sudo ufw enable
sudo ufw status
```

### 1.3 Instalacja Docker Engine

```bash
curl -fsSL https://get.docker.com | sudo bash
sudo usermod -aG docker $USER
newgrp docker         # lub wyloguj się i zaloguj ponownie
docker --version      # sprawdzenie
docker compose version
```

---

## 2. Pobieranie kodu

```bash
git clone https://github.com/YOUR_USERNAME/Typer.git /opt/typer
cd /opt/typer
```

---

## 3. Konfiguracja środowiska

```bash
cp deploy/.env.example deploy/.env
nano deploy/.env
```

Uzupełnij wartości w `deploy/.env`:

```env
DB_USER=typer
DB_PASSWORD=SILNE_HASLO_BAZY         # min. 20 znaków
DB_NAME=typerdb

# Wygeneruj: openssl rand -base64 64
JWT_SECRET=DLUGI_LOSOWY_KLUCZ_MIN_32_ZNAKI
JWT_ISSUER=Typer.Api
JWT_AUDIENCE=Typer.Client
JWT_EXPIRATION_MINUTES=120
```

---

## 4. Konfiguracja domeny w Nginx

Zamień `YOUR_DOMAIN` na swoją domenę we wszystkich plikach:

```bash
# Zastąp placeholder domeną (np. typer.example.com)
DOMAIN="typer.example.com"
sed -i "s/YOUR_DOMAIN/$DOMAIN/g" deploy/nginx/conf.d/typer.conf
```

---

## 5. Certyfikat SSL (Let's Encrypt)

### 5.1 Pierwsze uruchomienie bez SSL

Uruchom Nginx tylko na HTTP (do weryfikacji domeny):

```bash
# Tymczasowo odkomentuj blok server na port 80 bez redirect
# Albo użyj prostego kontenera certbot standalone:

docker run --rm -p 80:80 \
  -v /etc/letsencrypt:/etc/letsencrypt \
  certbot/certbot certonly \
  --standalone \
  --email twoj@email.com \
  --agree-tos \
  --no-eff-email \
  -d typer.example.com \
  -d www.typer.example.com
```

### 5.2 Sprawdź certyfikat

```bash
ls /etc/letsencrypt/live/typer.example.com/
# fullchain.pem  privkey.pem  ...
```

---

## 6. Uruchomienie aplikacji

```bash
cd /opt/typer
docker compose -f docker-compose.prod.yml --env-file deploy/.env up -d --build
```

Sprawdź status kontenerów:

```bash
docker compose -f docker-compose.prod.yml --env-file deploy/.env ps
```

Oczekiwane wyjście:

```
NAME            SERVICE    STATUS
typer-postgres  postgres   running (healthy)
typer-api       api        running (healthy)
typer-web       web        running (healthy)
typer-nginx     nginx      running (healthy)
typer-certbot   certbot    running
```

---

## 7. Migracje bazy danych

Jednorazowo po pierwszym uruchomieniu:

```bash
# Opcja A: przez dotnet-ef na hoście (wymaga .NET SDK)
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=typerdb;Username=typer;Password=TWOJE_HASLO"
dotnet ef database update \
  --project src/Typer.Infrastructure \
  --startup-project src/Typer.Api

# Opcja B: tymczasowy kontener z SDK
docker run --rm \
  --network typer_typer-internal \
  -e "ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=typerdb;Username=typer;Password=TWOJE_HASLO" \
  -v $(pwd):/app \
  -w /app \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet ef database update \
    --project src/Typer.Infrastructure \
    --startup-project src/Typer.Api
```

---

## 8. Dane testowe (opcjonalnie)

```bash
# Zaloguj się do kontenera PostgreSQL
docker exec -it typer-postgres psql -U typer -d typerdb

# Lub wykonaj skrypty przez psql
docker exec -i typer-postgres psql -U typer -d typerdb < scripts/seed-rounds.sql
docker exec -i typer-postgres psql -U typer -d typerdb < scripts/update-team-flags.sql
```

---

## 9. Logi i monitoring

```bash
# Wszystkie kontenery
docker compose -f docker-compose.prod.yml --env-file deploy/.env logs -f

# Konkretny kontener
docker logs -f typer-api
docker logs -f typer-web
docker logs -f typer-nginx

# Nginx access log
docker exec typer-nginx tail -f /var/log/nginx/access.log
```

---

## 10. Aktualizacja aplikacji

```bash
cd /opt/typer
git pull
bash deploy/deploy.sh
```

Lub bez przebudowy (tylko restart):

```bash
bash deploy/deploy.sh --skip-pull
```

---

## 11. Odnowienie certyfikatu SSL

Certbot odnawia certyfikat automatycznie (kontener `typer-certbot` sprawdza co 12h).

Ręczne odnowienie:

```bash
docker exec typer-certbot certbot renew
docker restart typer-nginx
```

---

## 12. Backup bazy danych

```bash
# Backup
docker exec typer-postgres pg_dump -U typer typerdb | gzip > backup_$(date +%Y%m%d).sql.gz

# Restore
gunzip -c backup_20260605.sql.gz | docker exec -i typer-postgres psql -U typer -d typerdb
```

Zautomatyzowany backup (cron):

```bash
crontab -e
# Dodaj (codziennie o 3:00):
0 3 * * * docker exec typer-postgres pg_dump -U typer typerdb | gzip > /opt/typer-backups/backup_$(date +\%Y\%m\%d).sql.gz
```

---

## 13. Zmienne środowiskowe — pełna lista

| Zmienna | Opis | Przykład |
|---|---|---|
| `DB_USER` | Użytkownik PostgreSQL | `typer` |
| `DB_PASSWORD` | Hasło PostgreSQL | silne hasło |
| `DB_NAME` | Nazwa bazy | `typerdb` |
| `JWT_SECRET` | Klucz podpisywania tokenów (min. 32 znaki) | `openssl rand -base64 64` |
| `JWT_ISSUER` | Issuer tokenu JWT | `Typer.Api` |
| `JWT_AUDIENCE` | Audience tokenu JWT | `Typer.Client` |
| `JWT_EXPIRATION_MINUTES` | Czas życia tokenu (minuty) | `120` |

---

## Struktura plików deployment

```
/
├── docker-compose.prod.yml      # produkcyjny Compose
├── deploy/
│   ├── .env.example             # szablon zmiennych
│   ├── .env                     # (NIE commituj!) rzeczywiste wartości
│   ├── deploy.sh                # skrypt wdrożenia
│   └── nginx/
│       ├── nginx.conf           # główna konfiguracja Nginx
│       └── conf.d/
│           └── typer.conf       # konfiguracja vhosta
└── src/
    ├── Typer.Api/Dockerfile     # obraz API
    └── Typer.Web/Dockerfile     # obraz Web
```

---

## Rozwiązywanie problemów

### Kontener nie startuje

```bash
docker logs typer-api
docker inspect typer-api
```

### Problem z certyfikatem

```bash
# Sprawdź czy port 80 jest dostępny z zewnątrz
curl -I http://typer.example.com
# Sprawdź logi certbot
docker logs typer-certbot
```

### Blazor nie ładuje WebSocket

Sprawdź czy w nginx.conf masz `map $http_upgrade $connection_upgrade` i blok `location /_blazor` z `proxy_set_header Upgrade $http_upgrade`.

### Baza danych niedostępna

```bash
docker exec -it typer-postgres psql -U typer -d typerdb
\dt   # lista tabel
```
