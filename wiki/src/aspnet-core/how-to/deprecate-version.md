{{#include ../../shared/how-to/deprecate-version-pre.md}}

This example demonstrates API versioning using all non-URL segment methods.

### Minimal API

```c#
var api = app.NewVersionedApi();
var hello = api.MapGroup( "/api/helloworld" )
               .HasDeprecatedApiVersion( 1.0 )
               .HasApiVersion( 2.0 );

hello.MapGet( "/", () => "Hello world!" );
hello.MapGet( "/", () => "Hello world v2.0!" ).MapToApiVersion( 2.0 );
```

### Mvc (Core)

```c#
[ApiController]
[ApiVersion( 2.0 )]
[ApiVersion( 1.0, Deprecated = true )]
[Route( "api/[controller]" )]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world!"

    [HttpGet, MapToApiVersion( 2.0 )]
    public string GetV2() => "Hello world v2.0!";
}
```


This example demonstrates API versioning using the URL segment method.

### Minimal API

```c#
var api = app.NewVersionedApi();
var hello = api.MapGroup( "/api/v{version:apiVersion}/helloworld" )
               .HasDeprecatedApiVersion( 1.0 )
               .HasApiVersion( 2.0 );

hello.MapGet( "/", () => "Hello world!" );
hello.MapGet( "/", () => "Hello world v2.0!" ).MapToApiVersion( 2.0 );
```

### MVC (Core)

```c#
[ApiController]
[ApiVersion( 2.0 )]
[ApiVersion( 1.0, Deprecated = true )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world!"

    [HttpGet, MapToApiVersion( 2.0 )]
    public string GetV2() => "Hello world v2.0!";
}
```

{{#include ../../shared/how-to/deprecate-version-post.md}}