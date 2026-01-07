# Testing Quick Start Guide

## Get Started in 5 Minutes

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

**Recommended**: Run test suites separately as shown above for cleaner output and to avoid connection pool exhaustion.

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

## Detailed Documentation

- [Full Testing Guide](./TESTING.md)
- [Backend Integration Tests](./backend/tests/Banking.Api.IntegrationTests/README.md)
- [E2E Tests Guide](./backend/tests/Banking.Api.E2ETests/README.md)

---

## Tips

1. **Run tests before committing** - Catch issues early
2. **Use watch mode for development** - TDD workflow
3. **Check E2E tests in browser** - Remove `--headless` in BaseTest.cs line 18 for visual debugging
4. **Save screenshots on failure** - Implement in E2E tests for CI/CD
5. **Generate coverage reports** - Use `--coverage` flag

---

## Resources

- [xUnit Documentation](https://xunit.net/docs/getting-started)
- [Vitest Documentation](https://vitest.dev/)
- [Selenium Documentation](https://www.selenium.dev/documentation/)
- [Vue Test Utils](https://test-utils.vuejs.org/)

---

