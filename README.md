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

## Consistency

Shuttle.Access is event-sourced (`Shuttle.Recall`): every mutation is appended to an event store, and the `Identity`,
`Permission`, `Role`, and `Tenant` read models used to answer queries are projections built from those events.
`Shuttle.Access.WebApi` can process a mutation in one of two ways, selected per-deployment by
`Shuttle:Recall:EventProcessing:ImmediateConsistency:Enabled`:

| | Immediate (`Enabled: true`, the web API's default) | Eventual (`Enabled: false`) |
| --- | --- | --- |
| Where the command runs | in-process, inside the `Shuttle.Access.WebApi` request | on the separate `Shuttle.Access.Server` process |
| How it gets there | dispatched straight to the `Shuttle.Mediator` participant | sent as a Hopper message to the server's inbox queue |
| When projections update | synchronously, before the HTTP response is returned | asynchronously, whenever the server's background event processor gets to it |
| Read-your-writes | guaranteed | **not** guaranteed — a query issued right after a write may still see the old state |
| Requires `Shuttle.Access.Server` to be running | no | yes |

`MessageDispatcher` (`Shuttle.Access.WebApi/MessageDispatcher.cs`) is what switches between the two — every mutating
endpoint calls it with both a Hopper message and the equivalent `Shuttle.Mediator` participant message, and it picks
one based on `RecallOptions.EventProcessing.ImmediateConsistency.Enabled`.

When immediate consistency is enabled, `Shuttle.Access.WebApi` also registers Recall's primitive event sequencer
in-process (`RegisterPrimitiveEventSequencing()`), since it can no longer rely on `Shuttle.Access.Server` to assign
sequence numbers to events it saves itself. `Shuttle.Access.Server` always registers it regardless, because it is the
one process guaranteed to be running whenever eventual consistency is in play.

If a projection handler throws while running immediately, the event is not lost — `Shuttle.Recall` raises
`EventProcessing.ImmediateConsistencyFailed`, and the event is still picked up and retried by the eventual event
processor on `Shuttle.Access.Server`'s next pass. See `Shuttle.Recall`'s
[Immediate Consistency](https://github.com/Shuttle/Shuttle.Recall#immediateconsistency-options) documentation for the
underlying mechanism; this section only covers how Shuttle.Access wires it up.

Prefer immediate consistency for a single-instance/all-in-one deployment, where guaranteeing that a client's next
query reflects its own write is worth doing the projection work on the request thread. Prefer eventual consistency
when scaling `Shuttle.Access.WebApi` out horizontally behind a load balancer — a single `Shuttle.Access.Server`
instance then owns all projection writes instead of every replica racing to apply the same event.

## Configuration

All Shuttle.Access-specific settings live under `Shuttle:Access` in `appsettings.json`. JWT issuer and OAuth provider
configuration are covered on the
[documentation site](https://www.pendel.co.za/shuttle-access/json-web-tokens.html); the tables below cover everything
else. Connection strings (`ConnectionStrings:Access`, `ConnectionStrings:azure`), `Shuttle:Hopper` (queue transports
and message routes), and `Shuttle:OAuth` follow standard `Shuttle.Hopper`/`Shuttle.OAuth` configuration.

### `Shuttle:Access` — shared (`Shuttle.Access`)

| Property | Default | Description |
| --- | --- | --- |
| `SystemTenantId` | `c3ee3908-716b-48df-abda-33b49e09be97` | Id of the built-in system tenant |
| `SystemTenantName` | `System` | Name of the built-in system tenant |
| `SystemAdministratorIdentityName` | `shuttle-admin` | Identity name seeded on first run |
| `SystemAdministratorPassword` | `shuttle-admin` | Password seeded on first run — change this |
| `SessionDuration` | `08:00:00` | How long a session is valid for once registered |
| `SessionRenewalTolerance` | `00:15:00` | Window before expiry within which a session is renewed rather than re-issued |

### `Shuttle:Access:Api` — `Shuttle.Access.WebApi`

| Property | Default | Description |
| --- | --- | --- |
| `AllowPasswordAuthentication` | `true` | Whether `POST /v1/sessions` accepts an identity name/password body |
| `OAuthRegisterUnknownIdentities` | `true` | Whether a successful OAuth sign-in registers a new identity when none exists |
| `ExtensionFolder` | `./.extension` | Folder Shuttle.Access.WebApi looks in for OAuth provider SVG icons (`{ExtensionFolder}/OAuth/{provider}.svg`) |

### `Shuttle:Access:Authorization` — `Shuttle.Access.AspNetCore` / `Shuttle.Access.WebApi`

The same section is bound by two different option classes: `Shuttle.Access.AspNetCore`'s `AccessAuthorizationOptions`
(used by every application, including the web API, to secure its own endpoints) and, only within
`Shuttle.Access.WebApi` itself, `AccessAuthenticationOptions` (used because the web API is the one deployment that
validates issuers and tokens). The properties each binds are disjoint, so nothing conflicts.

| Property | Bound by | Default | Description |
| --- | --- | --- | --- |
| `BaseAddress` | every app except `Shuttle.Access.WebApi` | *(empty)* | Address of the `Shuttle.Access.WebApi` deployment that resolves the caller's session |
| `Realm` | every app | `API` | Realm reported on a `401` challenge |
| `InsecureModeEnabled` | `Shuttle.Access.WebApi` only | `false` | Bypasses signature validation — never enable in production |
| `Issuers` | `Shuttle.Access.WebApi` only | `[]` | Accepted JWT issuers — see [JSON Web Tokens](https://www.pendel.co.za/shuttle-access/json-web-tokens.html) |

### `Shuttle:Access:Server` — `Shuttle.Access.Server`

| Property | Default | Description |
| --- | --- | --- |
| `MonitorKeepAliveInterval` | `00:00:15` | Interval at which the server's keep-alive heartbeat runs |
| `Timeout` | `00:02:00` | Keep-alive timeout before the server is considered unresponsive |

### `Shuttle:Access:SqlServer` — `Shuttle.Access.SqlServer`

| Property | Default | Description |
| --- | --- | --- |
| `ConnectionString` | *(empty)* | Overrides `ConnectionStrings:Access` when set |
| `CommandTimeout` | `00:00:30` | SQL command timeout |

### `Shuttle:Access:Client` — `Shuttle.Access.RestClient`

| Property | Default | Description |
| --- | --- | --- |
| `BaseAddress` | *(empty)* | Address of the `Shuttle.Access.WebApi` deployment to call |
| `RenewToleranceTimeSpan` | `00:05:00` | Window before session expiry within which the client renews it |

#### `Shuttle:Access:Client:PasswordAuthenticationInterceptor`

| Property | Default | Description |
| --- | --- | --- |
| `IdentityName` | *(empty)* | Identity used to authenticate this application as itself |
| `Password` | *(empty)* | Password for `IdentityName` |
| `TenantId` | *(none)* | Tenant to authenticate against, if not the system tenant |

### `Shuttle:Recall:EventProcessing:ImmediateConsistency` — consistency toggle

| Property | Default in `Shuttle.Access.WebApi` | Default in `Shuttle.Access.Server` | Description |
| --- | --- | --- | --- |
| `Enabled` | `true` | `false` | Selects immediate vs. eventual consistency — see [Consistency](#consistency) above |

# Documentation

Please visit the [Shuttle.Access documentation](https://www.pendel.co.za/shuttle-access/home.html) for more information.
