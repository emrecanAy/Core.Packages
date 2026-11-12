# Core.Packages

Reusable building blocks for ASP.NET Core APIs that follow a CQRS layout with MediatR.
Instead of rewriting the same plumbing in every project (paging, caching, logging, auth, error responses),
a new service references these libraries and only writes its own features.

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![MediatR](https://img.shields.io/badge/MediatR-13-555?style=flat-square)
![EF Core](https://img.shields.io/badge/EF%20Core-9-555?style=flat-square)
![License](https://img.shields.io/badge/license-Apache%202.0-blue?style=flat-square)

## What's inside

```
Core.Packages
├── Core.Application           MediatR pipeline behaviors, paging requests/responses
├── Core.Persistence           Generic EF Core repository, pagination, dynamic filtering
├── Core.Security              JWT, refresh tokens, password hashing, OTP & email 2FA
└── Core.CrossCuttingConcerns  Exception middleware (ProblemDetails), Serilog loggers
```

### Core.Application — pipeline behaviors

Cross-cutting rules are attached to a request by implementing a marker interface. Handlers stay focused on business logic.

| Behavior | Opt in with | What it does |
|---|---|---|
| `RequestValidationBehavior` | FluentValidation validator | Runs all validators before the handler and throws a `ValidationException` with the errors grouped by property |
| `CachingBehavior` | `ICachableRequest` | Reads/writes the response to `IDistributedCache` (sliding expiration, optional bypass) |
| `CacheRemovingBehavior` | `ICacheRemoverRequest` | Invalidates a single key or a whole **cache group** after a write |
| `TransactionScopeBehavior` | `ITransactionalRequest` | Wraps the handler in a `TransactionScope`; rolls back if it throws |
| `LoggingBehavior` | `ILoggableRequest` | Logs the request payload and the current user through Serilog |

Cache groups let a query like *"list brands, page 3"* be cached under its own key while still being
cleared together with every other brand list when a brand is created.

```csharp
public class GetListBrandQuery : IRequest<GetListResponse<BrandDto>>, ICachableRequest
{
    public PageRequest PageRequest { get; set; }

    public string CacheKey => $"GetListBrands({PageRequest.PageIndex},{PageRequest.PageSize})";
    public string? CacheGroupKey => "GetBrands";
    public bool BypassCache { get; }
    public TimeSpan? SlidingExpiration { get; }
}
```

### Core.Persistence — repository & dynamic queries

- `EfRepositoryBase<TEntity, TId, TContext>`: async CRUD on top of any `DbContext`, with `include`, `orderBy`,
  tracking control and a `withDeleted` switch that bypasses global query filters.
- `Entity<TId>` carries `CreatedDate`, `UpdatedDate` and `DeletedDate`, which the repository sets for you.
- `ToPaginateAsync()` returns a `Paginate<T>` with index, size, total count and `HasNext` / `HasPrevious`.
- **Dynamic filtering:** the client can send nested filter/sort trees as JSON. They are translated to
  LINQ with `System.Linq.Dynamic.Core`, using parameterised values rather than string concatenation.

```json
{
  "sort": [{ "field": "dailyPrice", "direction": "asc" }],
  "filter": {
    "field": "brand.name", "operator": "eq", "value": "BMW",
    "logic": "and",
    "filters": [{ "field": "modelYear", "operator": "gte", "value": "2020" }]
  }
}
```

Supported operators: `eq`, `neq`, `lt`, `lte`, `gt`, `gte`, `isnull`, `isnotnull`, `startswith`, `endswith`, `contains`, `doesnotcontain`.

### Core.Security

- JWT access tokens with role claims (`OperationClaim`) and refresh tokens
- Salted HMAC-SHA512 password hashing
- Two-factor authentication through an authenticator app (TOTP via Otp.NET) or an email code

### Core.CrossCuttingConcerns

- Global exception middleware that turns `BusinessException`, `ValidationException` and unexpected errors
  into [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) `ProblemDetails` responses
- Serilog logger base with file and MSSQL sinks, configured from `appsettings.json`

## Using it

Add the projects to your solution (or reference them as a git submodule) and register the behaviors once:

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddOpenBehavior(typeof(RequestValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(TransactionScopeBehavior<,>));
    cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
    cfg.AddOpenBehavior(typeof(CacheRemovingBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

services.AddStackExchangeRedisCache(o => o.Configuration = "localhost:6379"); // or AddDistributedMemoryCache()

app.ConfigureCustomExceptionMiddlware();
```

```json
"CacheSettings": { "SlidingExpiration": 2 }
```

## License

[Apache 2.0](LICENSE.txt)
