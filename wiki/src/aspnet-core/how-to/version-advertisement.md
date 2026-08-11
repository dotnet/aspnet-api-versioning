<!-- description: Aggregate and advertise ASP.NET Core API versions that are split across deployments behind a gateway. -->

{{#include ../../shared/how-to/version-advertisement-pre.md}}

```c#
[ApiVersion( 2.0 )]
[AdvertiseApiVersions( 1.0 )]
[ApiController]
[Route( "api/[controller]" )]
public class HelloWorld2Controller : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world v2.0!" );
}
```

```c#
[ApiVersion( 2.0 )]
[AdvertiseApiVersions( 1.0 )]
[ApiController]
[Route( "api/v{version:apiVersion}/helloworld" )]
public class HelloWorld2Controller : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world v2.0!" );
}
```

{{#include ../../shared/how-to/version-advertisement-post.md}}

## Mixing Minimal APIs with Controllers

Mixing existing controller-based APIs with Minimal APIs is a supported scenario, but the collation of API versions is
broken by default. This is simply because there is no intrinsic way to group controllers and Minimal APIs together.
However, by advertising API versions across implementations with the same name, the correct collation is possible.

```c#
[ApiController]
[ApiVersion( 1.0 )]
[AdvertiseApiVersions( 2.0 )]
[Route( "api/[controller]" )]
public class HelloWorld2Controller : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world v1.0!" );
}
```
_Figure 1: the controller-based API in 1.0_

```c#
var hello = app.NewVersionedApi();

hello.MapGet( "/api/helloworld", () => "Hello world v2.0!" )
     .HasApiVersion( 2.0 )
     .AdvertisesApiVersion( 1.0 );
```
_Figure 1: a minimal API in 2.0_

When [ApiVersioningOptions.ReportApiVersions] is enabled the controller and Minimal API implementations will both return
`api-supported-versions: 1.0, 2.0`.