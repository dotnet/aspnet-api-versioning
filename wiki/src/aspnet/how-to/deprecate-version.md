<!-- description: Mark an ASP.NET Web API version deprecated to advertise that it will become unsupported. -->

{{#include ../../shared/how-to/deprecate-version-pre.md}}

This example demonstrates API versioning using all non-URL segment methods.

```c#
[ApiVersion( 2.0 )]
[ApiVersion( 1.0, Deprecated = true )]
[RoutePrefix( "api/helloworld" )]
public class HelloWorldController : ApiController
{
    [Route]
    public string Get() => "Hello world!"

    [Route, MapToApiVersion( 2.0 )]
    public string GetV2() => "Hello world v2.0!";
}
```

This example demonstrates API versioning using the URL segment method.

```c#
[ApiVersion( 2.0 )]
[ApiVersion( 1.0, Deprecated = true )]
[RoutePrefix( "api/v{version:apiVersion}/helloworld" )]
public class HelloWorldController : ApiController
{
    [Route]
    public string Get() => "Hello world!"

    [Route, MapToApiVersion( 2.0 )]
    public string GetV2() => "Hello world v2.0!";
}
```

{{#include ../../shared/how-to/deprecate-version-post.md}}