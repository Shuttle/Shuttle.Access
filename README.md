# Shuttle.Access

An Identity and Access Management (IAM) platform providing fine-grained permissions in a session-based, multi-tenant
environment.  Identities may sign in using an identity name and password, or through a generic OAuth mechanism.

Shuttle.Access consists of a message-processing server, a restful web API, a Vue management front-end, and the NuGet
packages that secure *your* endpoints against it.

## Packages

The two client packages have a clean split of responsibility and neither references the other.  Pick whichever
matches what you are doing; a web API that does both takes both.

| Package | Direction | Purpose |
| --- | --- | --- |
| `Shuttle.Access.AspNetCore` | **inbound** | Secures *your* endpoints against the caller's credential — authentication handler, authorization middleware, and the `RequirePermission` / `RequireSession` requirements. |
| `Shuttle.Access.RestClient` | **outbound** | Calls the Shuttle.Access web API as *your own* identity via `IAccessClient`.  Depends on nothing but the contracts, so it works from a console application. |
| `Shuttle.Access.WebApi.Contracts` | — | Request/response contracts for the web API. |
| `Shuttle.Access.Messages` | — | Messages published/consumed by the Shuttle.Access server. |

```shell
dotnet add package Shuttle.Access.AspNetCore   # to secure your endpoints
dotnet add package Shuttle.Access.RestClient   # to call Shuttle.Access as yourself
```

## Securing an endpoint

`Shuttle.Access.AspNetCore` on its own is enough — securing endpoints needs no REST client:

```c#
builder.Services
    .AddAccessAuthorization(options =>
    {
        builder.Configuration.GetSection(AccessAuthorizationOptions.SectionName).Bind(options);

        options.BaseAddress = "http://localhost:5599";   // the Shuttle.Access web API
    });

// ...

app.UseAccessAuthorization();
```

There is nothing to configure beyond the address of the Shuttle.Access web API.  Your application never inspects the
credential it receives — it forwards the caller's `Authorization` header to `GET /v1/sessions/self` and takes the
session Shuttle.Access returns.  **Shuttle.Access is the only place where issuers and tokens are validated**, so
security configuration lives in one deployment instead of being duplicated across every application that trusts it.

A caller presents either a Shuttle.Access session token or a JSON Web Token in the `Authorization` header:

``` http
Authorization: Shuttle.Access token={GUID}
Authorization: Bearer {jwt}
```

An optional `Shuttle-Access-Tenant-Id` header selects the tenant; when it is absent the configured
`AccessOptions.SystemTenantId` is used.

## Calling Shuttle.Access as yourself

Securing endpoints answers "who is calling me?".  A separate question — "who am *I*, and what may I do?" — is what
`Shuttle.Access.RestClient` is for.  It always calls the web API under your application's own identity, so an
authentication provider is required; without one there would be no credential to send.

```c#
builder.Services
    .AddAccessClient(options =>
    {
        options.BaseAddress = "http://localhost:5599";
    })
    .UsePasswordAuthenticationProvider(providerBuilder =>
    {
        builder.Configuration.GetSection(PasswordAuthenticationInterceptorOptions.SectionName).Bind(providerBuilder.Options);
    });
```

To discover what your own identity may do, ask for its session — `HasPermission` is available directly on the
contract:

```c#
var response = await accessClient.Sessions.GetSelfAsync(cancellationToken);

if (response is { IsSuccessStatusCode: true, Content: not null } &&
    response.Content.HasPermission(tenantId, AccessPermissions.Identities.Register))
{
    await accessClient.Identities.PostAsync(registerIdentity, cancellationToken);
}
```

Every `IAccessClient` endpoint is available and each is authorized against the permissions assigned to *your*
identity.  Omitting `UseBearerAuthenticationProvider()` / `UsePasswordAuthenticationProvider()` fails the host at
startup rather than on the first outbound call.

### Console applications

`Shuttle.Access.RestClient` depends on nothing but the web API contracts and needs no incoming request, so a console
application uses exactly the registration above.  `AddAccessAuthorization()` plays no part — there is no caller to
authorize.

### The two are independent

| | Securing your endpoints | Calling Shuttle.Access as yourself |
| --- | --- | --- |
| Package | `Shuttle.Access.AspNetCore` | `Shuttle.Access.RestClient` |
| Credential | the caller's, forwarded as-is | your application's own |
| Endpoint(s) | `GET /v1/sessions/self` | the whole `IAccessClient` |
| Authorized against | the caller's permissions | your application's permissions |
| Entry point | `ISessionContext` (populated for you) | `IAccessClient` |
| Needs an HTTP request | yes | no |
| Needs an authentication provider | no | **yes** |

Neither package references the other, and a web API that does both simply registers both.  Adding an identity of your
own never changes how callers are authorized, because the caller's session is always resolved from the forwarded
header.

## Applying requirements

Minimal API endpoints use `RequirePermission` or `RequireSession`:

```c#
app.MapGet("/v1/customers", () =>
    {
        // Requires a specific permission.
    })
    .RequirePermission("crm://customers/view");

app.MapGet("/v1/customers/{id:guid}", (Guid id) =>
    {
        // No specific permission, but an active session has to exist.
    })
    .RequireSession();
```

MVC controllers use the equivalent attributes:

```c#
[HttpGet]
[RequirePermission("crm://customers/view")]
public IEnumerable<Customer> Get()
{
}

[HttpGet("{id:guid}")]
[RequireSession]
public Customer Get(Guid id)
{
}
```

A request with no session yields `401 Unauthorized`; a session without the required permission yields
`403 Forbidden`.

To check a permission on the *caller* in code, inject `ISessionContext` — it is populated during authentication and
carries the resolved session and tenant:

```c#
app.MapGet("/v1/categories", (ISessionContext sessionContext) =>
{
    if (!sessionContext.HasPermission("pim://categories/review"))
    {
        return Results.Forbid();
    }

    return Results.Ok();
});
```

## Permission structure

Permissions are assigned to roles, and roles to identities, per tenant:

```
.
├─ Permissions
│  ├─ *
│  ├─ system://context/read
│  └─ system://context/write
├─ Roles
│  ├─ Administrator
│  │  └─ Permissions
│  │     └─ *
│  ├─ Reader
│  │  └─ Permissions
│  │     └─ system://context/read
│  └─ Owner
│     └─ Permissions
│        ├─ system://context/read
│        └─ system://context/write
└─ Identity
   ├─ admin
   │  └─ Roles
   │     └─ Administrator
   ├─ someone@domain.com
   │  └─ Roles
   │     └─ Reader
   └─ mrresistor@example.co.za
      └─ Roles
         └─ Owner
```

# Documentation

Please visit the [Shuttle.Access documentation](https://www.pendel.co.za/shuttle-access/home.html) for more information.
