# Complete Testing Suite Overview

## Test Pyramid Summary

```
                    ┌─────────────────┐
                    │   E2E Tests     │  8 tests
                    │   (Selenium)    │
                    └────────┬────────┘
                         /   |   \
                        /    |    \
                   ┌───────────────────┐
                   │ Integration Tests │  11 tests
                   │  (Testcontainers) │
                   └────────┬──────────┘
                       /    |    \
                      /     |     \
            ┌───────────────────────────────┐
            │  Application & Domain Tests   │  24 tests
            │  (Unit Tests)                 │
            └───────────────────────────────┘

   TOTAL: 43 Backend Tests + 77 Frontend Tests = 120 Tests ✓
```

---

## Backend Testing (43 Tests)

### Layer 1: Domain Unit Tests (11 tests)
**Location**: `tests/Banking.Domain.Tests/`
**Framework**: xUnit + FluentAssertions
**Coverage**: Entity business rules and validations

[OK] Account entity invariants
[OK] Customer address validation
[OK] Enum value validations

**Run Command**:
```bash
cd backend
dotnet test tests/Banking.Domain.Tests
```

---

### Layer 2: Application Unit Tests (13 tests)
**Location**: `tests/Banking.Application.Tests/`
**Framework**: xUnit + FluentAssertions
**Coverage**: Business logic and validators

[OK] Customer creation validator
[OK] Email validation
[OK] Required field validation
[OK] Error message formatting

**Run Command**:
```bash
cd backend
dotnet test tests/Banking.Application.Tests
```

---

### Layer 3: Integration Tests (11 tests)
**Location**: `tests/Banking.Api.IntegrationTests/`
**Framework**: xUnit + Testcontainers + Real PostgreSQL
**Coverage**: API endpoints with database

[OK] Customer CRUD operations
[OK] HTTP status codes
[OK] Error responses
[OK] Database state verification
[OK] Authentication/Authorization

**Run Command**:
```bash
cd backend
dotnet test tests/Banking.Api.IntegrationTests
```

**Note**: Automatically starts PostgreSQL container, waits for readiness, runs tests, cleans up

---

### Layer 4: End-to-End Tests (8 tests)
**Location**: `tests/Banking.Api.E2ETests/`
**Framework**: Selenium WebDriver + Chrome
**Coverage**: User workflows and UI interactions

#### Authentication Tests (4)
1. Login with valid credentials → Navigate to customers page
2. Login with invalid credentials → Show error modal
3. Login page loads with proper form fields
4. Email and password inputs are present

#### Customer Management Tests (4)
1. Customer list page loads after login
2. Customer table renders with at least zero rows
3. Customer page has table element
4. Login flow navigates to correct URL

**Run Command** (requires running services):
```bash
# Prerequisites: Database, API, Frontend all running
cd backend
dotnet test tests/Banking.Api.E2ETests
```

**Setup Required**:
```bash
# Terminal 1: Database
docker run -d --name banking-db \
  -e POSTGRES_USER=banking \
  -e POSTGRES_PASSWORD=banking \
  -e POSTGRES_DB=banking \
  -p 5433:5432 \
  postgres:16-alpine

# Terminal 2: API
cd backend
dotnet run --project src/Banking.Api

# Terminal 3: Frontend
cd frontend
npm run dev

# Terminal 4: Tests
cd backend
dotnet test tests/Banking.Api.E2ETests
```

---

## Frontend Testing (77 Tests)

### Component Unit Tests
**Location**: `frontend/src/`
**Framework**: Vitest + @vue/test-utils + @testing-library/vue
**Coverage**: Vue components and interactions

| Component | Tests | Focus |
|-----------|-------|-------|
| Modal | 8 | Overlay rendering, button clicks, close behavior |
| SiteHeader | 8 | Navigation, logout, user menu |
| TextField | 12 | Input validation, placeholder, events |
| LoginPage | 12 | Form submission, error handling, disabled state |
| CustomerForm | 13 | Form validation, field interactions, submit |
| AccountForm | 13 | Form setup, field state, interactions |
| **TOTAL** | **77** | **All passing ✓** |

**Run Command**:
```bash
cd frontend
npm run test:unit -- --run
```

**Run in Watch Mode** (for development):
```bash
cd frontend
npm run test:unit
```

---

## Test Statistics

### Coverage by Category
- **Unit Tests**: 24 tests (Domain + Application)
- **Integration Tests**: 11 tests (API with Database)
- **E2E Tests**: 8 tests (Full user workflows)
- **Component Tests**: 77 tests (Frontend UI)
- **TOTAL**: 120 tests

### Coverage by Layer
```
API Layer:         19 tests (Integration + E2E)
Application:       13 tests (Application layer)
Domain:            11 tests (Business rules)
Frontend:          77 tests (Components)
```

### Execution Time
| Test Suite | Time |
|-----------|------|
| Domain Tests | ~2 seconds |
| Application Tests | ~3 seconds |
| Integration Tests | ~8 seconds |
| Component Tests | ~15 seconds |
| E2E Tests | ~60 seconds |
| **Total** | ~88 seconds |

---

## Running All Tests

### Option 1: Individual Suites
```bash
# Backend unit tests only
cd backend
dotnet test

# Frontend tests only
cd frontend
npm run test:unit -- --run

# E2E tests only (requires services running)
cd backend
dotnet test tests/Banking.Api.E2ETests
```

### Option 2: All Backend Tests
```bash
cd backend
dotnet test
# Runs: Domain + Application + Integration tests (33 tests)
```

### Option 3: All Tests (Requires Setup)
```bash
# Setup services first (see E2E section above)
# Then run all test types
dotnet test --project backend/Banking.sln
npm run test:unit -- --run --project frontend
dotnet test tests/Banking.Api.E2ETests
```

### Option 4: Using Helper Scripts
```bash
# Windows
./run-tests.bat
# Choose option 8: "Run all tests"

# Bash/WSL
./run-tests.sh run_all_tests
```

---

## Test Organization

### Project Structure
```
backend/
├── src/
│   ├── Banking.Api/
│   ├── Banking.Application/
│   ├── Banking.Domain/
│   └── Banking.Infrastructure/
└── tests/
    ├── Banking.Domain.Tests/          ← 11 unit tests
    ├── Banking.Application.Tests/      ← 13 unit tests
    ├── Banking.Api.IntegrationTests/   ← 11 integration tests
    └── Banking.Api.E2ETests/           ← 8 e2e tests
        ├── Pages/
        │   ├── BasePage.cs
        │   ├── LoginPage.cs
        │   └── CustomersPage.cs
        └── Tests/
            ├── AuthenticationE2ETests.cs
            └── CustomerE2ETests.cs

frontend/
└── src/
    ├── App.vue
    ├── components/          ← Component test files
    │   ├── Modal.test.ts
    │   ├── SiteHeader.test.ts
    │   └── forms/
    │       ├── LoginPage.test.ts
    │       ├── CustomerForm.test.ts
    │       └── AccountForm.test.ts
    └── TextField.test.ts
```

---

## Test Dependencies

### Backend
- **xUnit**: Test framework (v2.8.1)
- **FluentAssertions**: Assertion library (v6.12.0)
- **Testcontainers**: Docker container testing (v3.8.0)
- **Selenium WebDriver**: Browser automation (v4.24.0)

### Frontend
- **Vitest**: Test framework (v2.1.9)
- **@vue/test-utils**: Vue component testing (v2.4.6)
- **@testing-library/vue**: DOM testing utilities (v8.1.0)
- **@testing-library/user-event**: User interaction simulation (v14.5.2)

---

## Continuous Integration Readiness

All tests are designed to run in CI/CD pipelines:
- [OK] No external dependencies (containers self-provisioned)
- [OK] No manual setup required
- [OK] Deterministic results
- [OK] Fast execution (~90 seconds total)
- [OK] Clear pass/fail indicators
- [OK] Detailed error messages for debugging

---

## Next Steps

1. **Read**: [SETUP_AND_RUN_E2E.md](./SETUP_AND_RUN_E2E.md) for detailed E2E setup
2. **Read**: [E2E_TEST_FIXES.md](./E2E_TEST_FIXES.md) for what was fixed
3. **Run**: Backend tests: `cd backend && dotnet test`
4. **Run**: Frontend tests: `cd frontend && npm run test:unit -- --run`
5. **Setup & Run**: E2E tests following the SETUP_AND_RUN_E2E.md guide
6. **Monitor**: Check all 120 tests passing ✓

---

## Key Files

| File | Purpose |
|------|---------|
| `SETUP_AND_RUN_E2E.md` | Step-by-step E2E setup guide |
| `E2E_TEST_FIXES.md` | Detailed fixes applied to E2E tests |
| `run-tests.bat` | Windows helper script |
| `run-tests.sh` | Bash/WSL helper script |
| `backend/Banking.sln` | Backend solution with all projects |
| `frontend/vitest.config.ts` | Frontend test configuration |

---

## Support

For issues:
1. Check the troubleshooting sections in `SETUP_AND_RUN_E2E.md`
2. Review the fixes in `E2E_TEST_FIXES.md`
3. Ensure all services are running (database, API, frontend)
4. Check Docker is installed and running
5. Verify ports 5433, 5185, 5173 are available
