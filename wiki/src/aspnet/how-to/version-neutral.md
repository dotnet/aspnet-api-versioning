<!-- description: Make an ASP.NET Web API endpoint accept any API version, or none, for cases such as health checks. -->

{{#include ../../shared/how-to/version-neutral-pre.md}}

### Web API

```c#
[ApiVersionNeutral]
[RoutePrefix( "api/health" )]
public class HealthController : ApiController
{
    [HttpGet]
    [Route( "ping" )]
    public IHttpActionResult Ping() => Ok();
}
```

{{#include ../../shared/how-to/version-neutral-post.md}}

### Web API

```c#
[ApiVersionNeutral]
[RoutePrefix( "api/v{version:apiVersion}/health" )]
public class HealthController : ApiController
{
    [HttpGet]
    [Route( "ping" )]
    public IHttpActionResult Ping() => Ok();
}
```