<!-- description: Make an ASP.NET Core endpoint accept any API version, or none, for cases such as health checks. -->

{{#include ../../shared/how-to/version-neutral-pre.md}}

### Minimal API

```c#
var hello = app.NewVersionedApi();

hello.MapGet( "/api/health/ping", () => Results.Ok() ).IsApiVersionNeutral();
```

### MVC (Core)

```c#
[ApiVersionNeutral]
[ApiController]
[Route( "api/[controller]/[action]" )]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping() => Ok();
}
```

{{#include ../../shared/how-to/version-neutral-post.md}}

### Minimal API

```c#
var hello = app.NewVersionedApi();

hello.MapGet( "/api/v{version:apiVersion}/health/ping", () => Results.Ok() ).IsApiVersionNeutral();
```

### MVC (Core)

```c#
[ApiVersionNeutral]
[ApiController]
[Route( "api/v{version:apiVersion}/[controller]/[action]" )]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping() => Ok();
}
```