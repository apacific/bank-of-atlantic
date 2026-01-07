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
