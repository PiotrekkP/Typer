# Typer — Football Tipping League

A football prediction league application built with **ASP.NET Core**, **Blazor Server** and **PostgreSQL**, designed for the FIFA World Cup 2026.

## Features

- **User registration & login** (ASP.NET Identity + JWT)
- **Match predictions** — predict scores before kickoff; predictions lock when the match starts
- **Automatic scoring** — points are awarded automatically when a match status changes to Finished
- **Scoring rules** (configurable per season):
  - Correct winner: 2 pts
  - Correct winner + goal difference: 3 pts
  - Exact score: 5 pts
  - Favourite team multiplier: ×2
  - Favourite player goal bonus: +3 pts per goal
  - Tournament winner bonus: +20 pts
- **Rescore endpoint** — recalculates points if goal scorers are corrected
- **Live match view** — live minute counter + blinking green dot during InProgress matches
- **Ranking** with points breakdown by category
- **Player profile pages** with full prediction history
- **Dashboard** home page with upcoming matches, recent results, mini-ranking and user profile

## Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8 Web API (Minimal + Controllers) |
| UI | Blazor Server (.NET 8, InteractiveServer) |
| ORM | Entity Framework Core 8 + Npgsql |
| Database | PostgreSQL |
| Auth | ASP.NET Identity + JWT Bearer |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / API / Web) |

## Project Structure

```
src/
├── Typer.Domain/          # Entities, enums
├── Typer.Application/     # Interfaces, DTOs, service contracts
├── Typer.Infrastructure/  # EF Core, service implementations
├── Typer.Api/             # REST API (JWT auth)
└── Typer.Web/             # Blazor Server UI

scripts/                   # SQL seed scripts
tests/
└── Typer.Domain.Tests/
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL](https://www.postgresql.org/) (local or Docker)

### 1. Clone & configure

```bash
git clone https://github.com/YOUR_USERNAME/Typer.git
cd Typer
```

Copy `appsettings.json` templates and fill in your values:

```bash
# src/Typer.Api/appsettings.json
# src/Typer.Web/appsettings.json
```

Replace:
- `YOUR_DB_PASSWORD` → your PostgreSQL password
- `CHANGE_ME_TO_A_LONG_RANDOM_SECRET_MIN_32_CHARS` → a random JWT secret (≥ 32 characters)

### 2. Apply database migrations

```bash
dotnet ef database update --project src/Typer.Infrastructure --startup-project src/Typer.Api
```

### 3. Seed initial data (optional)

Run the SQL scripts in `scripts/` via pgAdmin or psql:

```bash
psql -U postgres -f scripts/seed-rounds.sql
psql -U postgres -f scripts/update-team-flags.sql
```

### 4. Run the applications

```bash
# Terminal 1 — API
dotnet run --project src/Typer.Api

# Terminal 2 — Blazor UI
dotnet run --project src/Typer.Web
```

Open [http://localhost:5118](http://localhost:5118) in your browser.

Swagger UI: [http://localhost:5165/swagger](http://localhost:5165/swagger)

## API Endpoints (selected)

| Method | Route | Description |
|---|---|---|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Login, returns JWT |
| GET | `/api/matches/rounds` | All rounds with matches |
| PATCH | `/api/matches/{id}/result` | Update match result + auto-score |
| POST | `/api/matches/{id}/rescore` | Recalculate points for a match |
| POST | `/api/scoring/tournament-winner/{teamId}` | Award tournament winner bonus |

## License

MIT
