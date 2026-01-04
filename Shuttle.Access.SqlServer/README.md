# Shuttle.Access.SqlServer

## Development

The `DesignTimeDbContextFactory` uses the `AddUserSecrets` to get the configuration.

Right-click on the `Shuttle.Access.Data` project and select `Manage User Secrets` and set the following configuration (using the relevant connection string parameters):

```json
{
  "ConnectionStrings": {
    "Access": "{your-connection-string}"
  }
}
```

## EF Core Migrations

Start migrations from scratch:

```
dotnet ef database drop --force
```

```
dotnet ef migrations remove
```

### Bundle

```
dotnet ef bundle
```