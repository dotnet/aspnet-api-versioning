# Asp.Versioning.Http

`Asp.Versioning.Http` adds API versioning to ASP.NET Core **Minimal APIs**. Use
`Asp.Versioning.Mvc` as well when the service has controllers.

## Set up

```csharp
using Asp.Versioning;

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning( options => options.ReportApiVersions = true );
```

`ReportApiVersions = true` adds the `api-supported-versions` and `api-deprecated-versions`
response headers. Without it, clients get no version information in responses.

## Version the endpoints

Create one versioned API from the app, then declare the versions each endpoint implements:

```csharp
var api = app.NewVersionedApi();

api.MapGet( "/items", () => "version 1" ).HasApiVersion( 1.0 );
api.MapGet( "/items", () => "version 2" ).HasApiVersion( 2.0 );

// no version required
api.MapGet( "/ping", () => "pong" ).IsApiVersionNeutral();
```

`MapGroup( ... ).HasApiVersion( ... )` on the object returned by `NewVersionedApi()` versions every
endpoint in the group. A request is routed by the `api-version` query string parameter, for example
`GET /items?api-version=2.0`.

## Behavior you can rely on

- A request with **no** version to a versioned endpoint fails with **400** (`unspecified`).
- A version no endpoint supports fails with **400** (`unsupported`); a value that is not a version
  (`api-version=banana`) fails with **400** (`invalid`). Both are `application/problem+json`.
- `api-version=1` is read as `1.0`.
- Call `NewVersionedApi()` on the app itself. Calling it on a `MapGroup` throws
  `InvalidOperationException` ("A versioned API group cannot be mapped as a nested group.").

## More

- Change where clients send the version, or assume a default version:
  [version sources and defaults](version-sources-and-defaults.md).
- Deprecate a version or change error responses:
  [deprecation and errors](deprecation-and-errors.md).
