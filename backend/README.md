# GeoBingo backend

The backend provides authentication, lobby discovery, real-time gameplay, persistence, and telemetry for GeoBingo. It targets .NET 10 and is organized as five projects in `GeoBingo.slnx`.

## Projects

| Project                  | Responsibility                                                                                     |
| ------------------------ | -------------------------------------------------------------------------------------------------- |
| `GeoBingo.Api`           | ASP.NET Core host, REST controllers, SignalR hub, authentication, health checks, and lobby runtime |
| `GeoBingo.Contracts`     | Shared REST, SignalR, lobby, and game-mode contracts                                               |
| `GeoBingo.Data`          | Entity Framework Core context, PostgreSQL mappings, repositories, and migrations                   |
| `GeoBingo.GameModes`     | Game-mode abstractions, registry, and Capture Challenge implementation                             |
| `GeoBingo.Observability` | Structured logging, metrics, tracing, and optional OTLP export                                     |

REST endpoints handle authentication, game-mode discovery, and lobby creation or lookup. Gameplay commands and projections travel through SignalR. Active lobby and round state is process-local; PostgreSQL currently persists authentication identities and users.

## Local development

Prerequisites:

- .NET 10 SDK
- PostgreSQL
- Google OAuth client credentials

Complete the shared [Google Cloud setup](../README.md#google-cloud-setup) first. The OAuth web client created for that project provides the client ID and client secret used below.

From `backend`, restore the repository's local tools:

```bash
dotnet tool restore
```

Update the local values in [`GeoBingo.Api/appsettings.json`](GeoBingo.Api/appsettings.json):

| Setting                                       | Local value                                                                        |
| --------------------------------------------- | ---------------------------------------------------------------------------------- |
| `Authentication.Provider.Google.ClientId`     | Google OAuth client ID                                                             |
| `Authentication.Provider.Google.ClientSecret` | Google OAuth client secret                                                         |
| `Authentication.AllowedOrigins[0]`            | `http://localhost:5173`                                                            |
| `ConnectionStrings.DefaultConnection`         | `Host=localhost;Port=5432;Database=geobingo;Username=geobingo;Password=<password>` |

Apply the existing migrations:

```bash
dotnet ef database update \
  --project GeoBingo.Data/GeoBingo.Data.csproj \
  --startup-project GeoBingo.Api/GeoBingo.Api.csproj
```

Run the API with its HTTP launch profile:

```bash
dotnet run --project GeoBingo.Api/GeoBingo.Api.csproj --launch-profile http
```

## Common commands

Regenerate the frontend's typed SignalR contracts after changing annotated hub contracts:

```bash
dotnet tsrts \
  --project GeoBingo.Contracts/GeoBingo.Contracts.csproj \
  --output ../frontend/src/lib/generated/realtime \
  --serializer json \
  --naming-style camelCase \
  --enum name
```

Do not edit the generated TypeScript files manually.

## Observability

Structured JSON logs are written to the console by default. To export logs, traces, and metrics to an OTLP-compatible collector, set:

```text
Observability__Otlp__Enabled=true
Observability__Otlp__Endpoint=https://collector.example.com
```

The endpoint must be an absolute HTTP or HTTPS URL. OTLP export remains disabled when `Observability:Otlp:Enabled` is false.
