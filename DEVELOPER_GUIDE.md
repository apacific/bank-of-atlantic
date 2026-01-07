# Developer Quick Reference Guide

## Code Standards & Conventions

### Frontend (Vue 3 + TypeScript)

**JSDoc Comment Template:**
```typescript
/**
 * Brief description of what this does.
 * 
 * @param {Type} paramName - Parameter description
 * @returns {Type} Return value description
 * @example
 * functionName(arg) // Example usage
 * 
 * @throws {Error} When something fails
 */
function functionName(paramName: Type): Type {
  // implementation
}
```

**Vue Component Template:**
```vue
<script setup lang="ts">
/**
 * Props for the Component.
 * @property {Type} prop1 - Description
 */
defineProps<{
  prop1: Type;
}>();

/**
 * Emits for the Component.
 * @event event-name - Description
 */
const emit = defineEmits<{ (e: "event-name"): void }>();
</script>
```

**CSS Ordering** (Alphabetical within each rule block):
```css
.selector {
  align-items: center;      /* a */
  background: #fff;         /* b */
  border: 1px solid #ddd;   /* b */
  cursor: pointer;           /* c */
  display: flex;            /* d */
  /* ... continue alphabetically */
}
```

**Running Linting:**
```bash
npm run lint      # Auto-fix all linting issues
npm run build     # Build for production
npm run dev       # Development server
npm run test:unit # Run unit tests
```

### Backend (C# + .NET)

**XML Documentation Template:**
```csharp
/// <summary>
/// Brief description of the class or method.
/// </summary>
/// <remarks>
/// Additional details about behavior, important notes.
/// </remarks>
public class ClassName
{
    /// <summary>
    /// Describes this property.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Describes what this method does.
    /// </summary>
    /// <param name="paramName">Parameter description</param>
    /// <returns>Return value description</returns>
    /// <response code="200">Success response</response>
    /// <response code="400">Bad request</response>
    /// <exception cref="CustomException">When X occurs</exception>
    public IActionResult MethodName(string paramName)
    {
        // implementation
    }
}
```

**Controller Endpoint Template:**
```csharp
/// <summary>
/// Creates a new resource.
/// </summary>
/// <response code="201">Resource created successfully</response>
/// <response code="400">Validation failed</response>
/// <response code="401">Unauthorized</response>
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateRequest body, CancellationToken ct)
{
    // implementation
}
```

---

## File Structure & Organization

### Frontend Directory Structure
```
src/
├── app/                    # Application-level services
│   └── api.ts             # Axios instance and utilities
├── components/            # Shared components
│   ├── forms/            # Form components
│   │   ├── SelectField.vue
│   │   └── TextField.vue
│   ├── Modal.vue
│   └── SiteHeader.vue
├── features/             # Feature modules
│   ├── accounts/         # Account management
│   │   ├── components/
│   │   ├── pages/
│   │   └── validation/
│   ├── auth/            # Authentication
│   │   ├── pages/
│   │   └── authStore.ts
│   └── customers/       # Customer management
│       ├── components/
│       ├── pages/
│       └── validation/
├── App.vue             # Root component
├── main.ts             # Application entry point
├── router.ts           # Vue Router configuration
└── style.css          # Global styles
```

### Backend Directory Structure
```
src/
├── Banking.Api/                   # API Layer
│   ├── Controllers/              # REST endpoints
│   ├── Middleware/               # Request/response processing
│   └── Program.cs               # Application setup
├── Banking.Application/          # Application Layer
│   ├── Accounts/                # Account use cases
│   │   ├── Commands/            # Create, Update, Delete
│   │   ├── Queries/             # Get, List
│   │   ├── DTOs/                # Data Transfer Objects
│   │   └── Validation/          # FluentValidation rules
│   ├── Customers/               # Customer use cases
│   ├── Common/
│   │   ├── Errors/              # Custom exceptions
│   │   └── Validation/          # Shared validators
│   └── DependencyInjection.cs   # Service registration
├── Banking.Domain/               # Domain Layer
│   ├── Accounts/                # Account aggregate
│   └── Customers/               # Customer aggregate
└── Banking.Infrastructure/       # Infrastructure Layer
    ├── Identity/                # User authentication
    └── Persistence/             # Database context
```

---

## Common Tasks

### Adding a New Frontend Feature

1. **Create Feature Folder:**
   ```bash
   mkdir -p src/features/myfeature/{components,pages,validation}
   ```

2. **Create Files:**
   - `validation/myfeatureSchema.ts` - Zod schemas
   - `components/MyFeatureForm.vue` - Form component
   - `pages/MyFeaturePage.vue` - Page component

3. **Add JSDoc Comments:**
   - Document all props, emits, functions
   - Include usage examples

4. **Format CSS Alphabetically:**
   - Use `.prettierrc.json` config for formatting

### Adding a New Backend Endpoint

1. **Create Domain Model** (if needed):
   - `Banking.Domain/MyFeature/MyEntity.cs`

2. **Create Command/Query:**
   - `Banking.Application/MyFeature/Commands/CreateMyEntityCommand.cs`
   - Implement `IRequestHandler<>`

3. **Create Validator:**
   - `Banking.Application/MyFeature/Validation/CreateMyEntityValidator.cs`
   - Extend `AbstractValidator<>`

4. **Create Controller Endpoint:**
   ```csharp
   /// <summary>Endpoint description</summary>
   /// <response code="201">Success</response>
   [HttpPost]
   public async Task<IActionResult> Create([FromBody] CreateRequest body, CancellationToken ct)
   {
       var result = await mediator.Send(new CreateCommand(...), ct);
       return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
   }
   ```

5. **Add XML Documentation**

### Fixing Linting Issues

```bash
# Frontend
npm run lint    # Auto-fixes ESLint and Prettier issues

# Backend  
# Use Visual Studio's built-in code analysis or StyleCop analyzers
# Edit code to follow C# conventions (PascalCase for public members, etc.)
```

### Running Tests

```bash
# Frontend Unit Tests
npm run test:unit     # Run all tests
npm run test:ui       # Run with interactive UI

# Backend
dotnet test          # From backend directory
```

---

## Common Patterns

### Frontend Form Pattern
```typescript
// 1. Define initial state
const form = reactive({ field1: "", field2: "" });
const errors = reactive<Record<string, string>>({});

// 2. Validate with Zod
const schema = z.object({
  field1: z.string().min(1, "Required"),
  field2: z.number().positive("Must be positive"),
});

// 3. Handle submission
async function submit() {
  const parsed = schema.safeParse(form);
  if (!parsed.success) {
    for (const issue of parsed.error.issues) {
      errors[String(issue.path[0])] = issue.message;
    }
    return;
  }

  try {
    await api.post("/endpoint", form);
    // Show success, navigate, etc.
  } catch (err: any) {
    // Map API errors to form fields
  }
}
```

### Backend CQRS Handler Pattern
```csharp
public sealed record CreateCommand(string Name, string Email) : IRequest<ResultDto>;

public sealed class CreateCommandHandler : IRequestHandler<CreateCommand, ResultDto>
{
    private readonly IBankingDbContext db;
    
    public CreateCommandHandler(IBankingDbContext db) => this.db = db;
    
    public async Task<ResultDto> Handle(CreateCommand command, CancellationToken ct)
    {
        // Validate (done via pipeline behavior)
        // Create entity
        var entity = new MyEntity(command.Name, command.Email);
        
        // Save
        db.MyEntities.Add(entity);
        await db.SaveChangesAsync(ct);
        
        // Return
        return new ResultDto(entity.Id, entity.Name);
    }
}
```

### API Error Handling Pattern (Frontend)
```typescript
try {
  const res = await api.post("/endpoint", data);
  // Handle success
} catch (e: any) {
  // e is normalized ApiError with:
  // - status: number
  // - title: string
  // - detail: string
  // - errors: Record<string, string[]> (field-level)
  
  if (e.status === 400) {
    // Map field errors to form
    for (const [field, messages] of Object.entries(e.errors)) {
      formErrors[field] = messages[0];
    }
  } else if (e.status === 401) {
    // Handle unauthorized
    auth.clearSession();
    router.push("/login");
  }
}
```

---

## Performance Tips

### Frontend:
- Use `computed()` for derived state
- Implement proper key binding in v-for loops
- Lazy-load routes with dynamic imports
- Use CSS Grid/Flexbox instead of absolute positioning

### Backend:
- Use async/await with proper cancellation tokens
- Index frequently queried fields in database
- Use IQueryable for lazy evaluation
- Implement proper pagination for large result sets

---

## Troubleshooting

### Linting Issues Won't Fix
```bash
# Restart ESLint server in VS Code
Cmd+Shift+P > "ESLint: Restart ESLint Server"

# Or manually format file
npm run lint
```

### TypeScript Compilation Errors
```bash
# Check for type errors
npm run build

# View detailed error messages
npx vue-tsc -b
```

### Backend Build Issues
```bash
# Clean and rebuild
dotnet clean
dotnet build

# Restore packages if needed
dotnet restore
```

---

## Resources

- **Vue 3 Docs:** https://vuejs.org/
- **TypeScript:** https://www.typescriptlang.org/
- **ESLint:** https://eslint.org/
- **Prettier:** https://prettier.io/
- **C# .NET:** https://docs.microsoft.com/dotnet/
- **Entity Framework Core:** https://docs.microsoft.com/en-us/ef/core/
- **MediatR:** https://github.com/jbogard/MediatR
- **FluentValidation:** https://fluentvalidation.net/

---

## Contact & Support

For questions about code standards or improvements:
1. Check the [CODE_IMPROVEMENTS.md](CODE_IMPROVEMENTS.md) document
2. Review existing code examples in the codebase
3. Consult project documentation in [QUICKSTART.md](QUICKSTART.md)
