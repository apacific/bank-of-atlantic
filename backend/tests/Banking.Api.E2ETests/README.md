# Banking POC - End-to-End Testing with Selenium

## Overview

This E2E test suite uses **Selenium WebDriver** with **C#** and **xUnit** to validate complete user workflows across the full stack (API, Backend, Frontend).

## Architecture

### Page Object Model (POM)
Tests use the Page Object Model pattern for maintainability:
- `BasePage.cs` - Base class with common WebDriver operations
- `LoginPage.cs` - Login page interactions
- `CustomersPage.cs` - Customer management page interactions
- Extensible for additional pages (AccountsPage, etc.)

### Test Classes
- `AuthenticationE2ETests.cs` - Login flows, authentication validation
- `CustomerE2ETests.cs` - Customer CRUD operations, workflow testing

## Prerequisites

### Required Services Running
1. **Backend API** - Running on `http://localhost:5185`
2. **Frontend App** - Running on `http://localhost:5173`
3. **PostgreSQL Database** - Running (for test data)

### Test Account
- **Email:** `admin@bank.com`
- **Password:** `Admin@123`
- (Create via Integration Tests if needed)

## Running E2E Tests

### Build the Project
```bash
cd backend
dotnet build
```

### Run All E2E Tests
```bash
dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj
```

### Run Specific Test Class
```bash
dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj --filter "ClassName=AuthenticationE2ETests"
```

### Run Specific Test
```bash
dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj --filter "Name=Login_WithValidCredentials_ShouldNavigateToCustomersPage"
```

### Verbose Output
```bash
dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj --verbosity detailed
```

## Test Scenarios Covered

### Authentication Tests
- ✓ Valid credentials → Navigate to customers page
- ✓ Invalid credentials → Show error modal
- ✓ Empty email → Form validation
- ✓ Page elements verification

### Customer Management Tests
- ✓ Create customer with valid data
- ✓ Customer appears in list after creation
- ✓ Customers page loads after login
- ✓ Modal has required fields

## Configuration

### Browser Options
Modify `BaseTest.cs` to change browser behavior:

```csharp
var options = new ChromeOptions();
options.AddArgument("--no-sandbox");
options.AddArgument("--disable-dev-shm-usage");
// Uncomment for headless mode (CI/CD)
// options.AddArgument("--headless");
```

### Timeouts
- **Implicit Wait:** 5 seconds (BaseTest.cs line 23)
- **Explicit Wait:** 10 seconds (WebDriverWait in BasePage.cs)
- **Thread.Sleep:** Used for page navigation (2 seconds)

### Base URL
Change in `BaseTest.cs`:
```csharp
protected string BaseUrl { get; } = "http://localhost:5173";
```

## Future Enhancements

### Additional Test Scenarios
- [ ] Account creation and management
- [ ] Account type switching (Checking/Credit Card)
- [ ] Transaction operations
- [ ] Role-based access control validation
- [ ] Logout functionality
- [ ] Session expiration

### Improvements
- [ ] Data-driven tests using xUnit theories
- [ ] Screenshot capture on test failure
- [ ] Test reporting (HTML reports)
- [ ] Parallel test execution
- [ ] CI/CD integration (GitHub Actions, Azure Pipelines)

## Troubleshooting

### "No match found for location" Warnings
- These are Vue Router warnings, not test failures
- Occurs during router initialization in tests
- Safe to ignore

### ChromeDriver Issues
- Ensure ChromeDriver version matches Chrome browser version
- Package `Selenium.WebDriver.ChromeDriver` auto-updates with NuGet

### "Element not found" Errors
- Verify selectors match current UI structure
- Use browser DevTools to inspect elements
- Check wait times if page loads slowly
- Ensure frontend is running on port 5173

### Port Conflicts
- API: Change port in `Banking.Api.http` and `appsettings.json`
- Frontend: Change port in `vite.config.ts`
- Update `BaseUrl` in E2E tests accordingly

## Best Practices

1. **Use Page Objects** - Encapsulate selectors and interactions
2. **Explicit Waits** - Prefer `WebDriverWait` over `Thread.Sleep`
3. **Meaningful Test Names** - Follow given-when-then pattern
4. **Single Responsibility** - Each test validates one behavior
5. **Setup/Teardown** - Use constructor for setup, `Dispose()` for cleanup
6. **Test Data** - Use unique identifiers (timestamps) to avoid conflicts

## Integration with CI/CD

### GitHub Actions Example
```yaml
- name: Run E2E Tests
  run: |
    dotnet test tests/Banking.Api.E2ETests/Banking.Api.E2ETests.csproj \
      --logger "console;verbosity=detailed" \
      --no-build
```

## Related Documentation

- **Unit Tests:** `tests/Banking.Domain.Tests` (Domain logic)
- **Integration Tests:** `tests/Banking.Api.IntegrationTests` (API endpoints with real DB)
- **Component Tests:** `frontend/tests/unit` (Vue components)
