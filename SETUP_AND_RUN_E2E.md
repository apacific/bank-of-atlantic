# E2E Testing Setup and Execution Guide

## Prerequisites

Before running E2E tests, all three services must be running:
1. **PostgreSQL Database** (Port 5433)
2. **Backend API** (Port 5185)
3. **Frontend Application** (Port 5173)
4. **Chrome Browser** (For Selenium WebDriver)

## Step 1: Start PostgreSQL Database

```bash
docker run -d --name banking-db \
  -e POSTGRES_USER=banking \
  -e POSTGRES_PASSWORD=banking \
  -e POSTGRES_DB=banking \
  -p 5433:5432 \
  postgres:16-alpine
```

Verify database is running:
```bash
docker ps | grep banking-db
```

## Step 2: Start Backend API

From the backend directory:

```bash
cd c:\projects\software\fullstack\banking-poc\backend
dotnet run --project src/Banking.Api
```

Expected output should include:
```
Now listening on: http://localhost:5185
```

The API will automatically:
- Apply EF Core migrations
- Seed test data with admin user (admin@bank.com / Admin@123)
- Create database tables

## Step 3: Start Frontend Application

From a new terminal, from the frontend directory:

```bash
cd c:\projects\software\fullstack\banking-poc\frontend
npm install  # (only if you haven't done this yet)
npm run dev
```

Expected output should include:
```
VITE v5.0.0  ready in XXX ms

➜  Local:   http://localhost:5173/
```

## Step 4: Verify All Services Are Running

1. **Database**: `docker ps` shows `banking-db` running
2. **API**: `http://localhost:5185/swagger/index.html` opens successfully
3. **Frontend**: `http://localhost:5173/` shows login page
4. **Login**: Can login with admin@bank.com / Admin@123

## Step 5: Run E2E Tests

From the backend directory:

```bash
cd c:\projects\software\fullstack\banking-poc\backend
dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj -v detailed
```

### Expected Test Results

**4 Authentication Tests:**
- [OK] Login_WithValidCredentials_ShouldNavigateToCustomersPage
- [OK] Login_WithInvalidCredentials_ShouldShowErrorModal
- [OK] LoginPage_ShouldHaveProperElements
- [OK] Login_ShouldShowFieldsForEmailAndPassword

**4 Customer Management Tests:**
- [OK] CustomersPage_ShouldBeAccessibleAfterLogin
- [OK] CustomerList_ShouldHaveAtLeastZeroCustomers
- [OK] CustomerPage_ShouldRenderTable
- [OK] LoginFlow_ShouldNavigateToCustomersUrl

**Total: 8 tests should pass**

## Troubleshooting

### Tests Timeout or Fail - Connection Issues

**Problem**: Chrome driver can't connect or times out waiting for elements

**Solution**: 
1. Ensure frontend is running on port 5173
2. Check Chrome/Chromium is installed: `where google-chrome` or `where chromium`
3. Check that frontend login page loads: Visit http://localhost:5173/ manually
4. Increase wait timeouts in BaseTest.cs if needed

### Login Not Working

**Problem**: Test enters credentials but login doesn't complete

**Solution**:
1. Verify API is running on port 5185
2. Check API logs for errors
3. Ensure database is running with correct credentials
4. Verify user exists: Check database directly or API logs

### Can't Find Elements

**Problem**: Selenium can't find buttons, input fields, etc.

**Solution**:
1. Open frontend in Chrome DevTools
2. Inspect element selectors to match in CustomersPage.cs and LoginPage.cs
3. Update selectors if UI structure has changed
4. Common selectors:
   - Email input: `input[type='email']`
   - Password input: `input[type='password']`
   - Submit button: `button[type='submit']`
   - Table: `table`

### Cleanup

To stop services:

```bash
# Stop frontend (Ctrl+C in terminal)
# Stop API (Ctrl+C in terminal)
# Stop database
docker stop banking-db
docker rm banking-db
```

## Architecture

### Test Hierarchy
```
BaseTest (abstract)
├── BaseTest.cs (WebDriver setup, navigation)
├── BasePage (abstract)
│   ├── LoginPage (login form interactions)
│   └── CustomersPage (customer list interactions)
├── AuthenticationE2ETests
└── CustomerE2ETests
```

### Test Data

Default credentials created during API startup:
- **Email**: admin@bank.com
- **Password**: Admin@123
- **Role**: Admin

## Performance Notes

- First test run takes longer (Chrome driver initialization)
- Each test creates a new ChromeDriver instance
- Tests include 1-3 second waits for navigation and UI updates
- Total test suite runs in ~60 seconds

## Next Steps

After E2E tests pass:
1. [OK] All 112+ tests passing (35 backend + 77 frontend + 8 e2e)
2. Run full test suite: `dotnet test backend/Banking.sln --logger "console;verbosity=detailed"`
3. Measure coverage: `dotnet test /p:CollectCoverage=true`
4. Deploy to staging environment
