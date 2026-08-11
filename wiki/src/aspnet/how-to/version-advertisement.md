<!-- description: Aggregate and advertise ASP.NET Web API versions that are split across deployments behind a gateway. -->

{{#include ../../shared/how-to/version-advertisement-pre.md}}

```c#
[ApiVersion( 2.0 )]
[AdvertiseApiVersions( 1.0 )]
[Route( "api/helloworld" )]
public class HelloWorld2Controller : ApiController
{
    [HttpGet]
    public string Get() => "Hello world v2.0!" );
}
```

```c#
[ApiVersion( 2.0 )]
[AdvertiseApiVersions( 1.0 )]
[Route( "api/v{version:apiVersion}/helloworld" )]
public class HelloWorld2Controller : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world v2.0!" );
}
```

{{#include ../../shared/how-to/version-advertisement-post.md}}