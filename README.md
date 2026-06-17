# GLMS — Global Logistics Management System

**Student:** Ronewa Maselesele | **Student Number:** ST10157545  
**Module:** Enterprise Application Development (EAPD)  
**Programme:** Higher Certificate in Cloud & Software Development — The IIE Varsity College  
**Submission Date:** June 2026

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Solution Architecture](#2-solution-architecture)
3. [Technology Stack](#3-technology-stack)
4. [Project Structure](#4-project-structure)
5. [Getting Started](#5-getting-started)
   - [Prerequisites](#51-prerequisites)
   - [Option A — Docker Compose (Recommended)](#52-option-a--docker-compose-recommended)
   - [Option B — Local Development](#53-option-b--local-development)
6. [Default Credentials](#6-default-credentials)
7. [API Reference](#7-api-reference)
8. [Running Tests](#8-running-tests)
9. [Database](#9-database)
10. [Part 1 — Architecture Report Summary](#10-part-1--architecture-report-summary)
11. [Part 2 — Monolith Prototype Summary](#11-part-2--monolith-prototype-summary)
12. [Part 3 — SOA Refactoring Summary](#12-part-3--soa-refactoring-summary)
13. [Design Patterns Implemented](#13-design-patterns-implemented)
14. [Connecting with SSMS / Azure Data Studio](#14-connecting-with-ssms--azure-data-studio)
15. [Environment Variables Reference](#15-environment-variables-reference)
16. [Known Issues and Limitations](#16-known-issues-and-limitations)
17. [Submission Links](#17-submission-links)
18. [References](#18-references)

---

## 1. Project Overview

TechMove Logistics is a global shipping coordinator currently relying on a legacy system of
spreadsheets, emails, and manual phone calls. This has resulted in severe data fragmentation,
lost invoices, compliance failures on expired contracts, and significant workflow bottlenecks.

The **Global Logistics Management System (GLMS)** is a three-part enterprise-grade web
platform that replaces this legacy system with a cloud-native, containerised, service-oriented
architecture. It manages international freight contracts, driver schedules, service invoices,
and real-time currency conversion from USD to ZAR.

### Business Problems Solved

| Problem | GLMS Solution |
|---|---|
| Lost and untracked contracts | Centralised Contract Management Hub with PDF storage |
| No contract status enforcement | Automated workflow: Draft → Active → Expired → OnHold |
| Manual currency conversion | Live USD→ZAR conversion via ExchangeRate API |
| Fragmented service requests | Service requests validated against active contracts only |
| No audit trail | Exchange rate, timestamps, and user actions all persisted |
| No scalability path | Containerised SOA with Docker Compose and health checks |

---

## 2. Solution Architecture

The system is split into three independently deployable containers communicating over
an internal Docker bridge network.

```
┌─────────────────────────────────────────────────────────────┐
│                        HOST MACHINE                         │
│                                                             │
│  Browser ──► http://localhost:5000                          │
│                      │                                      │
│         ┌────────────▼────────────┐                         │
│         │   glms-frontend-web     │  Container 3            │
│         │   ASP.NET Core MVC      │  Port 5000              │
│         │   (NO direct DB access) │                         │
│         └────────────┬────────────┘                         │
│                      │ HttpClient calls                      │
│         ┌────────────▼────────────┐                         │
│         │   glms-backend-api      │  Container 2            │
│         │   ASP.NET Core Web API  │  Port 8080              │
│         │   JWT + Swagger UI      │  Swagger at /           │
│         └────────────┬────────────┘                         │
│                      │ EF Core + migrations                  │
│         ┌────────────▼────────────┐                         │
│         │   glms-sqlserver        │  Container 1            │
│         │   SQL Server 2019       │  Port 1433              │
│         │   Express Edition       │  SSMS: localhost,1433   │
│         └─────────────────────────┘                         │
└─────────────────────────────────────────────────────────────┘
```

### Container Communication
Containers address each other by **Docker service name**, not `localhost`:

| From → To | Address used |
|---|---|
| `glms.api` → SQL Server | `sqlserver,1433` |
| `glms.web` → API | `http://glms.api:8080` |
| Your browser → API Swagger | `http://localhost:8080` |
| Your browser → MVC UI | `http://localhost:5000` |
| SSMS / Azure Data Studio | `localhost,1433` |

---

## 3. Technology Stack

| Layer | Technology | Version |
|---|---|---|
| Backend API | ASP.NET Core Web API | .NET 8 |
| Frontend UI | ASP.NET Core MVC | .NET 8 |
| Database | SQL Server | 2019 Express |
| ORM | Entity Framework Core | 8.0.0 |
| Authentication | JWT Bearer (HS256) | System.IdentityModel.Tokens.Jwt 7.3.1 |
| API Documentation | Swagger / Swashbuckle | 6.6.2 |
| Containerisation | Docker + Docker Compose | v3.9 |
| Unit Testing | xUnit + Moq | 2.6.6 / 4.20.70 |
| Integration Testing | WebApplicationFactory + EF InMemory | 8.0.0 |
| Currency API | ExchangeRate-API (open.er-api.com) | Free tier — no key required |
| CSS Framework | Bootstrap | 5.3.2 |

---

## 4. Project Structure

```
GLMS/
├── GLMS.sln                          ← Visual Studio solution
├── docker-compose.yml                ← Full 3-container orchestration
├── .env                              ← Secret env vars (never commit this)
├── .gitignore
├── README.md
├── DatabaseMigrations.sql            ← Standalone SQL fallback script
├── sql-init/
│   ├── 01_init_glms_db.sql           ← DB creation + seed data (Docker init)
│   └── init-db.sh                    ← Shell runner for init script
│
├── GLMS.API/                         ── Backend Web API (NEW in Part 3)
│   ├── GLMS.API.csproj
│   ├── Program.cs                    ← DI, JWT, Swagger, EF, CORS, auto-migrate
│   ├── Dockerfile
│   ├── appsettings.json              ← LocalDB connection string
│   ├── appsettings.Docker.json       ← Docker container connection string
│   ├── GLMS.API.http                 ← VS Code / Rider HTTP request file
│   ├── Controllers/
│   │   ├── AuthController.cs         ← POST /api/auth/login → JWT
│   │   ├── ClientsController.cs      ← Full CRUD /api/clients
│   │   ├── ContractsController.cs    ← CRUD + PATCH status + PDF upload
│   │   ├── ServiceRequestsController.cs
│   │   ├── CurrencyController.cs     ← GET /api/currency/rate
│   │   └── DashboardController.cs    ← GET /api/dashboard
│   ├── Models/
│   │   ├── Models.cs                 ← Client, Contract, ServiceRequest, AppUser
│   │   └── DTOs.cs                   ← Request/response shapes
│   ├── Data/
│   │   ├── ApplicationDbAPIContext.cs
│   │   └── Migrations/
│   └── Services/
│       ├── JwtService.cs
│       ├── CurrencyService.cs        ← open.er-api.com + 60-min cache + fallback
│       └── FileService.cs            ← PDF-only validation + GUID storage
│
├── GLMS.Web/                         ── MVC Frontend (REFACTORED in Part 3)
│   ├── GLMS.Web.csproj               ← No EF Core packages — API calls only
│   ├── Program.cs                    ← HttpClient + Session (stores JWT)
│   ├── Dockerfile
│   ├── appsettings.json              ← ApiBaseUrl: http://localhost:8080
│   ├── Controllers/
│   │   └── Controllers.cs            ← Auth, Home, Clients, Contracts, SRs
│   ├── Models/
│   │   └── Dtos.cs                   ← Mirror of API DTOs (no EF dependency)
│   ├── Services/
│   │   └── ApiClient.cs              ← Single HttpClient wrapper for all API calls
│   └── Views/                        ← All Razor views: CRUD, Login, Dashboard
│
└── GLMS.Tests/                       ── Test project (33 tests)
    ├── GLMS.Tests.csproj             ← xUnit + Moq + InMemory + WebApplicationFactory
    ├── Dockerfile
    └── GlmsTests.cs
        ├── CurrencyServiceTests       (8 tests)
        ├── FileServiceTests           (7 tests)
        ├── ContractWorkflowTests      (5 tests)
        └── ApiIntegrationTests        (13 tests)
```

---

## 5. Getting Started

### 5.1 Prerequisites

| Tool | Version | Notes |
|---|---|---|
| Docker Desktop | 4.x+ | https://www.docker.com/products/docker-desktop |
| .NET SDK | 8.0+ | https://dot.net — only needed for local dev |
| Visual Studio 2022 | 17.8+ | Community edition is free |
| SSMS or Azure Data Studio | Any | Optional — for DB inspection |

### 5.2 Option A — Docker Compose (Recommended)

This is the fastest way to run the full system with a single command.

```bash
# 1. Clone the repository
git clone <your-github-repo-url>
cd GLMS

# 2. Start all 3 containers (builds images on first run)
docker compose up --build
```

Docker Compose starts services in dependency order:

```
sqlserver (health check) → glms.api (health check) → glms.web
```

SQL Server takes approximately 30 seconds to initialise on first boot.
EF Core migrations are applied automatically by `glms.api` on startup.

**Access the application:**

| Service | URL |
|---|---|
| MVC Web UI | http://localhost:5000 |
| API + Swagger UI | http://localhost:8080 |
| SQL Server (SSMS) | localhost,1433 |

**Useful commands:**
```bash
# View running containers and health status
docker compose ps

# View API logs
docker compose logs glms.api -f

# View all logs
docker compose logs -f

# Run tests only (without starting the full stack)
docker compose run --rm glms.tests

# Stop everything
docker compose down

# Stop and remove all data (full reset)
docker compose down -v
```

### 5.3 Option B — Local Development

Run each service directly from Visual Studio or the .NET CLI.

**Step 1 — Apply database migrations (API project):**
```bash
cd GLMS.API
dotnet ef database update
```
This creates `GLMS_DB` on `(localdb)\mssqllocaldb` and seeds the test data.

**Step 2 — Run the API:**
```bash
cd GLMS.API
dotnet run
# API starts at http://localhost:8080
# Swagger UI at http://localhost:8080
```

**Step 3 — Run the MVC frontend (separate terminal):**
```bash
cd GLMS.Web
dotnet run
# UI starts at http://localhost:5000
```

**Step 4 — Run tests:**
```bash
cd GLMS.Tests
dotnet test --verbosity normal
```

> **Note:** `GLMS.Web/appsettings.json` must have `"ApiBaseUrl": "http://localhost:8080"` for
> local development. This is already set correctly by default.

---

## 6. Default Credentials

| Field | Value |
|---|---|
| Username | `admin` |
| Password | `Admin@1234` |

The admin user is seeded automatically on first database migration.  
To test JWT authentication directly, use the Swagger UI at `http://localhost:8080`:
1. Call `POST /api/auth/login` with the credentials above
2. Copy the `token` from the response
3. Click **Authorize** in the Swagger header and paste the token

---

## 7. API Reference

All endpoints are documented interactively in the Swagger UI at `http://localhost:8080`.

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/login` | No | Returns signed JWT (8hr expiry) |
| `GET` | `/api/dashboard` | No | Aggregate counts for dashboard |
| `GET` | `/api/clients` | No | All clients |
| `POST` | `/api/clients` | JWT | Create client |
| `PUT` | `/api/clients/{id}` | JWT | Update client |
| `DELETE` | `/api/clients/{id}` | JWT | Delete client (no contracts) |
| `GET` | `/api/contracts?status=&from=&to=` | No | Filtered contracts |
| `POST` | `/api/contracts` | JWT | Create contract |
| `PUT` | `/api/contracts/{id}` | JWT | Update contract |
| `PATCH` | `/api/contracts/{id}/status` | JWT | Update status only |
| `POST` | `/api/contracts/{id}/agreement` | JWT | Upload PDF (max 10 MB) |
| `GET` | `/api/contracts/{id}/agreement` | No | Download PDF |
| `DELETE` | `/api/contracts/{id}` | JWT | Delete contract + PDF |
| `GET` | `/api/servicerequests` | No | All service requests |
| `POST` | `/api/servicerequests` | JWT | Create SR (validates contract) |
| `PATCH` | `/api/servicerequests/{id}/status` | JWT | Update SR status |
| `DELETE` | `/api/servicerequests/{id}` | JWT | Delete SR |
| `GET` | `/api/currency/rate` | No | Live USD/ZAR rate |

### Contract Status Workflow

```
Draft ──► Active ──► Expired
  │          │
  └──────────┴──► OnHold ──► Active
```

Service requests can only be created on **Active** or **Draft** contracts.
Attempts on `Expired` or `OnHold` contracts return `400 Bad Request`.

### Using the HTTP Request File

A `GLMS.API.http` file is included in the `GLMS.API` project folder.
Open it in Visual Studio, VS Code (REST Client extension), or JetBrains Rider.

1. Send the **Login** request first — it is named `@name login`
2. The `@authToken` variable is automatically populated from the response
3. All subsequent JWT-protected requests reuse `Authorization: Bearer {{authToken}}`

---

## 8. Running Tests

```bash
cd GLMS.Tests
dotnet test --verbosity normal
```

All 33 tests pass without network access and without a running SQL Server instance.

| Test Class | Count | Coverage |
|---|---|---|
| `CurrencyServiceTests` | 8 | USD→ZAR math, rounding, zero/negative guards, parameterised theory data |
| `FileServiceTests` | 7 | PDF accepted; .exe, .docx, MIME-spoof rejected; null file; unique GUIDs; delete |
| `ContractWorkflowTests` | 5 | All 4 `ContractStatus` values vs `CanCreateServiceRequest` |
| `ApiIntegrationTests` | 13 | Auth, Dashboard, Clients, Contracts, SRs, Currency, Swagger endpoint |
| **Total** | **33** | |

**Integration test approach:** `WebApplicationFactory<Program>` replaces SQL Server
with `EF Core InMemory` — tests run in-process without Docker or a real database,
making them safe for CI/CD pipelines.

---

## 9. Database

### Schema

```
Clients
  Id (PK), Name, ContractDetails, Region, CreatedOn

Contracts
  Id (PK), ClientId (FK → Clients), StartDate, EndDate
  Status (Draft|Active|Expired|OnHold), ServiceLevel
  SignedAgreementPath, SignedAgreementFileName, CreatedOn

ServiceRequests
  Id (PK), ContractId (FK → Contracts), Description
  CostUsd, CostZar, ExchangeRateUsed, Status, CreatedOn

Users
  Id (PK), Username, PasswordHash, Role

__EFMigrationsHistory
  MigrationId, ProductVersion
```

### Seed Data

The following records are created automatically on first migration:

| Entity | Records |
|---|---|
| Users | `admin` / `Admin@1234` |
| Clients | Acme Freight Ltd (EMEA), FastTrack Logistics (SADC), Global Ship Co (APAC) |
| Contracts | Contract #1 (Active), Contract #2 (Expired) |

### Migration Options

**Option A — EF Core (automatic on startup):**
The API applies pending migrations on every startup inside a retry loop (useful for Docker
where SQL Server may not be ready immediately).

**Option B — EF CLI:**
```bash
cd GLMS.API
dotnet ef database update
```

**Option C — Pure SQL (Azure Portal / SSMS):**
Open `DatabaseMigrations.sql` and run the three labelled migration blocks in order.

---

## 10. Part 1 — Architecture Report Summary

Part 1 established the enterprise architecture blueprint presented to the TechMove Board of Directors.

**Framework:** Zachman Framework (chosen over TOGAF for its artefact-centric structure, directly addressing TechMove's data fragmentation problem)

**Zachman Mapping (Data × Function × Network, Planner and Designer rows):**

| | Data (What) | Function (How) | Network (Where) |
|---|---|---|---|
| **Planner** | Contracts, Clients, SRs, Invoices, Currencies | Create Contract, Raise SR, Convert Currency, Track Status | Global Offices, Azure Region, Client Portals |
| **Designer** | Contract (ID, SLA, Status, Value), ServiceRequest | ContractService, ForexService (Strategy), NotificationService (Observer) | Load Balancer → API Gateway → Microservices → Redis → SQL Replicas |

**Three GoF Design Patterns selected:**
1. **Observer** — Contract status change notifications
2. **Strategy** — Pluggable currency conversion providers
3. **Factory Method** — Service request type creation

**NFR Commitments:**
- 99.9% availability (< 8.7 hrs downtime/year)
- Automatic failover < 30 seconds
- OpenAPI 3.0 interoperability
- OAuth 2.0 / JWT authentication
- Redis caching + horizontal pod autoscaling

---

## 11. Part 2 — Monolith Prototype Summary

Part 2 delivered a fully working ASP.NET Core MVC monolith.

**Delivered:**
- SQL Server database with EF Core (Client, Contract, ServiceRequest entities)
- PDF upload (dual extension + MIME type validation, GUID naming, 10 MB limit)
- Contract workflow enforcement (`CanCreateServiceRequest` — blocks Expired/OnHold)
- LINQ search/filter by date range and status
- Live USD→ZAR currency conversion (open.er-api.com, 60-min cache, R18.50 fallback)
- 26 xUnit unit tests (currency math, file validation, workflow, LINQ filters)
- Pure SQL migration script (`DatabaseMigrations.sql`)

**Not yet in Part 2 (delivered in Part 3):**
- Web API separation
- JWT authentication
- Swagger documentation
- Docker containerisation
- Integration tests

---

## 12. Part 3 — SOA Refactoring Summary

Part 3 decoupled the monolith into the current three-container SOA.

**Key changes:**
- `GLMS.API` — new project; all business logic and database access moved here
- `GLMS.Web` — refactored to remove all `DbContext` injections; now uses `ApiClient` (HttpClient wrapper) exclusively
- JWT authentication added (HS256, 8-hour expiry, Bearer token in session)
- Swagger/OpenAPI 3.0 with JWT support enabled on the API
- Docker Compose with 3 containers, health checks, named volumes, and internal networking
- Test suite expanded from 26 unit tests to 33 (added 7 integration tests via `WebApplicationFactory`)

**Architecture pattern:**
```
Presentation Layer (GLMS.Web)  ←→  Service Layer (GLMS.API)  ←→  Data Layer (SQL Server)
```

---

## 13. Design Patterns Implemented

### Observer — Contract Status Notifications
- **Interface:** `IContractObserver` with `Update(status)` method
- **Subject:** `ContractSubject` maintains a list of observers and calls `Notify()` on status change
- **Concrete Observers:** `EmailNotifier`, `AuditLogNotifier`
- **Where used:** Contract status transitions trigger downstream notifications without coupling the Contract entity to notification logic

### Strategy — Currency Conversion
- **Interface:** `ICurrencyService` / `IConversionStrategy`
- **Context:** `CurrencyService` delegates to the configured strategy
- **Concrete Strategies:** `OpenExrStrategy` (primary — open.er-api.com), fallback rate R18.50
- **Where used:** `ServiceRequestsController.Create` calls `GetUsdToZarRateAsync()` + `ConvertUsdToZar()`

### Factory Method — Service Request Creation
- **Abstract Creator:** `ServiceRequestCreator` with abstract `CreateRequest()` and shared `ValidateContract()`
- **Concrete Creators:** `FreightSRCreator`, `ExpressSRCreator`, `CustomsSRCreator`
- **Product Interface:** `IServiceRequest`
- **Where used:** SR creation logic is subclassed per request type, eliminating conditional branching

---

## 14. Connecting with SSMS / Azure Data Studio

When running via Docker Compose, SQL Server is exposed on port `1433` of your host machine.

**Connection settings:**

| Field | Value |
|---|---|
| Server | `localhost,1433` |
| Authentication | SQL Server Authentication |
| Login | `sa` |
| Password | `YourStrong!Passw0rd` |
| Database | `GLMS_DB` |

> **Important:** Uncheck "Trust server certificate" or add `TrustServerCertificate=True`
> to avoid SSL errors with SQL Server 2019 in Docker.

---

## 15. Environment Variables Reference

All secrets are configured in `.env` (Docker Compose) or `appsettings.json` (local dev).
**Never commit `.env` to version control.**

| Variable | Default | Description |
|---|---|---|
| `SA_PASSWORD` | `YourStrong!Passw0rd` | SQL Server SA password — must meet complexity rules |
| `JWT_KEY` | `GLMS-SuperSecret-JWT-Key-TechMove-2026!@#$` | HS256 signing key |
| `Jwt__Issuer` | `glms-api` | JWT issuer claim |
| `Jwt__Audience` | `glms-clients` | JWT audience claim |
| `ApiBaseUrl` | `http://localhost:8080` | URL the MVC frontend uses to call the API |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Docker` | Controls Swagger visibility |
| `DOCKER_REGISTRY` | _(empty)_ | Optional registry prefix for image names |

---

## 16. Known Issues and Limitations

| Issue | Status | Notes |
|---|---|---|
| Swagger OpenApi.Models namespace error | Resolved | Pin `Swashbuckle.AspNetCore` to **6.6.2** and add `Microsoft.OpenApi` **1.6.14** explicit reference. Versions 7.x+ use OpenApi 2.0 which breaks the classic `Microsoft.OpenApi.Models` namespace |
| `ApplicationDbAPIContext` vs `ApplicationDbContext` naming | Resolved | All controllers and `Program.cs` must use the **same type name** in `AddDbContext<T>()` and constructor injection |
| SQL Server takes ~30s to start in Docker | Expected | The API retries migration up to 10 times with 3-second gaps; the health check prevents the frontend starting too early |
| `DEFAULT_DATABASE` env var has no effect | Resolved | Removed — database name is set in the connection string directly |
| PDF upload in Docker requires volume | Configured | `agreements_data` named volume maps to `/app/wwwroot/uploads/agreements` |
| Tests Dockerfile path | Note | Dockerfile references `GLMS.Tests/Dockerfile` — ensure folder name matches exactly |

---

## 17. Submission Links

| Deliverable | Link |
|---|---|
| GitHub Repository | _(paste your GitHub URL here)_ |
| Video Walkthrough | _(paste your YouTube link here)_ |
| Part 1 Architecture Report | `GLMS_Part1_Architecture_Report_ST10157545.docx` |
| Part 2 POE Report | `EAPD_POE_PART2_ST10157545.docx` |

---

## 18. References

Fowler, M. (2002) *Patterns of Enterprise Application Architecture.* Boston: Addison-Wesley.

Freeman, E. and Robson, E. (2020) *Head First Design Patterns: Building Extensible and
Maintainable Object-Oriented Software.* 2nd edn. Sebastopol: O'Reilly Media.

Gamma, E., Helm, R., Johnson, R. and Vlissides, J. (1994) *Design Patterns: Elements of
Reusable Object-Oriented Software.* Reading, MA: Addison-Wesley.

Hardt, D. (2012) *The OAuth 2.0 Authorization Framework.* RFC 6749. Internet Engineering
Task Force (IETF). Available at: https://datatracker.ietf.org/doc/html/rfc6749

Hohpe, G. and Woolf, B. (2003) *Enterprise Integration Patterns: Designing, Building, and
Deploying Messaging Solutions.* Boston: Addison-Wesley.

Lankhorst, M. (2017) *Enterprise Architecture at Work: Modelling, Communication and
Analysis.* 4th edn. Berlin: Springer.

Newman, S. (2019) *Monolith to Microservices: Evolutionary Patterns to Transform Your
Monolith.* Sebastopol: O'Reilly Media.

Richardson, L. and Ruby, S. (2007) *RESTful Web Services.* Sebastopol: O'Reilly Media.

Sessions, R. (2007) 'A Comparison of the Top Four Enterprise-Architecture Methodologies',
*Microsoft Developer Network.* Available at:
https://msdn.microsoft.com/en-us/library/bb466232.aspx

Shalloway, A. and Trott, J. (2004) *Design Patterns Explained: A New Perspective on
Object-Oriented Design.* 2nd edn. Boston: Addison-Wesley.

Zachman, J.A. (1987) 'A Framework for Information Systems Architecture', *IBM Systems
Journal,* 26(3), pp. 276–292. doi: 10.1147/sj.263.0276.

---

*© 2026 Ronewa Maselesele (ST10157545) — The IIE Varsity College. Confidential — For Academic Assessment Only.*
