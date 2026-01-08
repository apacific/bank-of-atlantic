# Bank of Atlantic

## About

Proof of concept for a banking application that models customers, accounts, and role-based operations over a PostgreSQL backing store.

The project is split into:

- `backend/` – ASP.NET Core Web API (C#, .NET 9, EF Core, Identity, JWT)
- `frontend/` – Vue 3 + TypeScript SPA (Vite, Vue Router, Axios)

The goal is to demonstrate a realistic architecture and a non-trivial feature set rather than a trivial CRUD sample.

---

## Features

### Domain and business rules

- Customers
  - Basic identity and mailing address data.
  - Uniqueness enforced on:
    - Email (normalized)
    - SSN/TIN (normalized)
  - Creates customer-since date on first creation.
  - Duplicate email / SSN/TIN returns `409 Conflict` with field-level errors (both violations reported in one response).

- Accounts
  - Account number is generated server-side; clients never provide it.
  - Date opened is set server-side to the current date; clients never provide it.
  - Account types:
    - Deposit: `Checking`, `Savings`, `MoneyMarket`, `CD`
    - Credit: `CreditCard`, `HELOC`, `PLOC`
  - Deposit accounts:
    - User sets an initial balance on creation.
    - Displayed as “Current balance”.
  - Credit accounts:
    - User sets an available credit limit on creation.
    - Displayed as “Available credit”.
  - Account deletion rules:
    - Only a Manager can delete accounts (role-based).
    - Business constraints enforced in the domain:
      - Deposit accounts can only be deleted if current balance is zero.
      - Credit accounts can only be deleted if the available credit limit rule is satisfied (for this POC: available balance must be zero).

### Authentication and authorization

- ASP.NET Core Identity with PostgreSQL store.
- JWT bearer authentication:
  - `/auth/login` accepts email + password and returns:
    - `accessToken`
    - `role` (`Employee` or `Manager`)
  - JWT includes a role claim (`ClaimTypes.Role`).
- Authorization:
  - Fallback policy requires authentication for all endpoints by default.
  - Explicit `[AllowAnonymous]` on login.
  - `[Authorize(Roles = "Manager")]` on account deletion endpoints.

### Frontend (Vue 3 + TypeScript)

- Login flow
  - Login page posts to `/auth/login`.
  - Stores JWT and role in `sessionStorage` (`boa_token`, `boa_role`).
  - Axios interceptor attaches `Authorization: Bearer <token>` to all requests.
  - Router guard:
    - Blocks protected routes when not authenticated and redirects to `/login`.
    - Redirects away from `/login` when already authenticated.

- Customer flows
  - `/customers` – list view:
    - Shows all customers with basic info and “customer since” date.
  - `/customers/:id` – detail view:
    - Full customer details.
    - Inline edit form for customer data.
    - Accounts section with:
      - Account number.
      - Friendly account type labels (for example, “Credit Card”, “Money Market”).
      - Balance label adjusted to the account type:
        - Deposit: “Current balance”.
        - Credit: “Available credit”.
      - “View details” button for each account.

- Account flows
  - Create account form:
    - Allows selecting account type.
    - For deposit accounts: initial balance field.
    - For credit accounts: available credit limit field.
  - `/customers/:customerId/accounts/:accountId` – account details view:
    - Shows account number, account type, date opened.
    - Shows either “Current balance” or “Available credit” with the appropriate value.
    - Edit form may adjust account type and balance/credit within the rules.
    - Delete button is visible only for Managers and is subject to backend rules.

- UI and layout
  - Global header with bank logo on the left; clicking the logo returns to the customers page.
  - Custom font stack:
    - Header title: `Archopada_Rounded_Oblique-SemBd.ttf`
    - Section headers: `Archopada_Rounded_Oblique-Black.ttf`
    - Body text: `Rubik-Medium.ttf`
  - SPA navigation via Vue Router.

---

## Technology stack

### Backend

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9 (Npgsql provider)
- ASP.NET Core Identity
- MediatR for application commands/queries
- FluentValidation (via validators in the application layer)
- PostgreSQL 16 (via Docker)
- Swashbuckle for OpenAPI / Swagger
- Custom exception middleware returning RFC 7807-style problem details

### Frontend

- Vue 3
- TypeScript
- Vite
- Vue Router
- Axios

---

## Project structure

High-level layout:

```text
backend/
  src/
    Banking.Api/            # HTTP API (controllers, Program.cs, middleware)
    Banking.Application/    # CQRS, DTOs, validators, application services
    Banking.Domain/         # Entities, value objects, enums
    Banking.Infrastructure/ # EF Core, Identity, migrations, DI wiring
  docker-compose.yml        # Postgres container definition

frontend/
  src/
    app/                    # Axios client, app bootstrap
    features/
      auth/                 # authStore, LoginPage
      customers/            # pages + forms
      accounts/             # account forms, pages, helpers
    components/             # shared UI (Modal, TextField, etc.)
    assets/fonts/           # custom fonts
  .env                      # Vite API base URL
```

---
## Testing
### Prerequisites
- .NET 9.0 SDK
- Node.js 18+
- Chrome/Chromium browser
- Docker (for integration tests)

### Step 1: Start Services
```bash
# Terminal 1: Backend API
cd backend
dotnet run --project src/Banking.Api

# Terminal 2: Frontend
cd frontend
npm install
npm run dev

# Terminal 3: Database (Docker)
docker run -d --name banking-db \
  -e POSTGRES_USER=banking \
  -e POSTGRES_PASSWORD=banking \
  -e POSTGRES_DB=banking \
  -p 5433:5432 \
  postgres:16-alpine

# Test Credentials (Seeded by API)
# Email: employee@atlantic.local
# Password: Passw0rd!
# Role: Employee
```

### Step 2: Run Unit Tests (No Dependencies)
```bash
cd backend
# Run domain + application unit tests (fast, no database)
dotnet test tests/Banking.Domain.Tests tests/Banking.Application.Tests
```

### Step 3: Run Integration Tests
```bash
cd backend
# Run integration tests with Testcontainers PostgreSQL
dotnet test tests/Banking.Api.IntegrationTests
```

### Step 4: Run Frontend Tests
```bash
cd frontend
npm run test:unit -- --run
```

### Step 5: Run E2E Tests (Backend + Frontend Running)
```bash
cd backend
dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj
```

---

## Run All Tests At Once

To run all tests together (may see database connection warnings during parallel execution):

```bash
cd backend
dotnet test
```

It's recommended to run test suites separately (as shown above) for cleaner output, and to avoid connection pool exhaustion.

---

## What's Being Tested

| Test Type | Count | Purpose |
|-----------|-------|---------|
| Domain Unit | 11 | Business rules |
| App Unit | 13 | Validation logic |
| API Integration | 11 | HTTP endpoints |
| Frontend Components | 77 | Vue components |
| E2E Selenium | 8+ | User workflows |

---

## Common Commands

### Backend
```bash
# Build
dotnet build

# Run all tests together
dotnet test

# Run unit tests only (fast, no database needed)
dotnet test tests/Banking.Domain.Tests tests/Banking.Application.Tests

# Run integration tests with test containers
dotnet test tests/Banking.Api.IntegrationTests

# Run E2E tests with Selenium (requires API, frontend, database running)
dotnet test tests/Banking.Api.E2ETests

# Run specific test file
dotnet test tests/Banking.Domain.Tests

# Run specific test
dotnet test --filter "Name=Login_WithValidCredentials_ShouldNavigateToCustomersPage"

# Verbose output
dotnet test --verbosity detailed
```

### Frontend
```bash
# Install dependencies
npm install

# Run tests once
npm run test:unit -- --run

# Watch mode
npm run test:unit

# With coverage
npm run test:unit -- --coverage
```

---

## Expected Results

### Backend Tests
```
Test Files  5 passed (5)
Tests  35 passed (35)
```

### Frontend Tests
```
Test Files  7 passed (7)
Tests  77 passed (77)
```

### E2E Tests (with services running)
```
Test Files  2 passed (2)
Tests  8+ passed (8+)
```

---

## Troubleshooting

### "Port already in use"
```bash
# Find and kill process using port 5185 (API)
lsof -i :5185 | grep LISTEN | awk '{print $2}' | xargs kill

# Or use docker
docker ps -a | grep banking-db
docker stop banking-db
docker rm banking-db
```

### "No match found for location" (E2E Tests)
- This is a Vue Router warning, safe to ignore
- Tests will still pass

### "Database unavailable" or "Service unavailable" errors during tests
- This occurs when running all tests in parallel (connection pool exhaustion)
- **Solution**: Run test suites separately (see steps above)
- Connection pool has been configured with `Maximum Pool Size=20; Minimum Pool Size=5`

### "ChromeDriver version mismatch"
```bash
cd backend/tests/Banking.Api.E2ETests
dotnet add package Selenium.WebDriver.ChromeDriver --force
```

### Database connection refused
```bash
# Check Docker
docker ps
# Verify connection on port 5433
nc -zv localhost 5433
# Restart if needed
docker stop banking-db
docker rm banking-db
# Then re-run the docker run command from Step 1
```

---

## Resources

- [xUnit Documentation](https://xunit.net/docs/getting-started)
- [Vitest Documentation](https://vitest.dev/)
- [Selenium Documentation](https://www.selenium.dev/documentation/)
- [Vue Test Utils](https://test-utils.vuejs.org/)

---

