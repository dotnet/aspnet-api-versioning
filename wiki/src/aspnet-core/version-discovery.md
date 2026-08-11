<!-- description: Advertise supported and deprecated API versions from an ASP.NET Core service via response headers. -->


{{#include ../shared/version-discovery.md}}

### Minimal API

```c#
using static Microsoft.AspNetCore.Http.HttpMethods;

// OPTIONS ~/api/myservice?api-version=[1.0|2.0|3.0]
app.MapMethods("/api/myservice", [Options], ( HttpContext context ) =>
{
    context.Response.Headers.Allow = new( [Get, Post, Options] );
    return Results.Ok();
});
```

```http
HTTP/2 200
allow: GET, POST, OPTIONS
api-supported-versions: 1.0, 2.0, 3.0
```

### MVC (Core)

```c#
using static Microsoft.AspNetCore.Http.HttpMethods;

// OPTIONS ~/api/myservice?api-version=[1.0|2.0|3.0]
[HttpOptions]
public IActionResult Options()
{
    Response.Headers.Allow = new( [Get, Post, Options] );
    return Ok();
}
```

```http
HTTP/2 200
allow: GET, POST, OPTIONS
api-supported-versions: 1.0, 2.0, 3.0
```