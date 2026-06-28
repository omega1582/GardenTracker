# 🌿 GardenTracker

A modern, multi-tenant garden management application featuring a layered .NET 10 clean architecture Web API backend, a React + TypeScript + Vite frontend, and SQL Server persistence.

---

## 🏗️ Architecture & Tech Stack

GardenTracker is built using a clean architecture pattern to ensure separation of concerns, testability, and maintainability.

```
       [GardenTracker.Web (React + Vite)]
                       │
                       ▼
       [GardenTracker.Api (Controllers & DTOs)]
                       │
                       ▼
    [GardenTracker.Services (Business Logic)]
             │                  │
             ▼                  ▼
    [GardenTracker.Data] ──► [GardenTracker.Core]
  (EF Migrations / Dapper)  (Entities / Interfaces)
```

### Backend (C# .NET 10)
- **GardenTracker.Core**: Plain Old CLR Objects (POCOs) representing domain entities (`Garden`, `Bed`, `Planting`, `InventoryItem`, etc.) and core service/repository interfaces. No external framework dependencies.
- **GardenTracker.Data**: Data access layer. Uses **EF Core** strictly for database schema modeling and migrations, and **Dapper** for high-performance runtime queries.
- **GardenTracker.Services**: Core business logic and multi-tenant security. Enforces ownership validation at every operation to ensure users can only access their own resources.
- **GardenTracker.Api**: REST API controllers, request/response DTOs, and JWT Authentication/Authorization.

### Frontend (React + TypeScript)
- **GardenTracker.Web**: Single Page Application (SPA) powered by React, TypeScript, Vite, and TailwindCSS (or Vanilla CSS for flexible styling).

### Database
- **SQL Server**: Multi-tenant relational schema. 
- **GardenTracker.Database**: SQL Server `.sqlproj` project for structural database visualization.

---

## 📂 Project Structure

```
GardenTracker/
├── GardenTracker.Core/       # Domain Entities & Interface definitions
├── GardenTracker.Data/       # DB Context, Dapper Repositories & Migrations
├── GardenTracker.Services/   # Business Logic & Ownership Checks
├── GardenTracker.Api/        # REST Controllers, DTOs & Program.cs
├── GardenTracker.Web/        # React + Vite Frontend
├── GardenTracker.Tests/      # Unit & Integration Tests (Testcontainers)
├── GardenTracker.Database/   # Database project (.sqlproj)
├── ANDROID_APP_PLAN.md       # Roadmap for the upcoming Kotlin mobile application
├── docker-compose.yml        # Docker orchestration config
├── run-dev.sh / .ps1         # Concurrently boots up the API and Web App in dev mode
└── .env.example              # Template for environment configuration
```

---

## 🚀 Getting Started

### Prerequisites
Make sure you have the following installed:
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v18+) & npm](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (required for running integration tests or containerized deployment)

### Environment Configuration
1. Copy the example environment template to create a `.env` file:
   ```bash
   cp .env.example .env
   ```
2. Open `.env` and configure your credentials (e.g., `DB_PASSWORD` and `JWT_KEY`).

### Local Development Setup

To run both the backend API and the frontend web app simultaneously with hot-reloading:

> [!NOTE]
> Make sure your database server is running. You can start the database via Docker if needed:
> `docker compose up -d db`

#### On Linux / macOS:
```bash
chmod +x run-dev.sh
./run-dev.sh
```

#### On Windows (PowerShell):
```powershell
./run-dev.ps1
```

Once running:
- **Backend API**: `http://localhost:5280`
- **Frontend Web**: `http://localhost:5173` (or the port specified by Vite in console output)

---

## 🐳 Running inside Docker Compose

To boot up the entire application stack (Database, API, and Web App) within Docker containers:

1. Ensure your `.env` file is set up.
2. Build and start the services:
   ```bash
   docker compose up --build
   ```
3. Access the application:
   - **Frontend UI**: `http://localhost:3000`
   - **Backend API**: `http://localhost:5280`

---

## 🧪 Testing

GardenTracker has a robust test suite split into two categories:

### 1. Unit Tests
Covers service-level business logic, ownership checks, and mapping. These do not require an active database connection and mock dependencies using Moq.
```bash
dotnet test --filter "Category!=Integration"
```

### 2. Integration Tests
Runs repository queries against a real SQL Server instance spun up dynamically inside a Docker container using **Testcontainers**.

> [!IMPORTANT]
> **Docker must be running** on your system to run integration tests, otherwise they will fail immediately.

```bash
dotnet test --filter "Category=Integration"
```

To run **all tests** (both unit and integration):
```bash
dotnet test
```

---

## 📱 Mobile App Roadmap
An Android client built with Kotlin and Jetpack Compose is currently planned.
