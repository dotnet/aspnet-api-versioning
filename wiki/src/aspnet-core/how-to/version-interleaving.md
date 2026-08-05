{{#include ../../shared/how-to/version-interleaving-pre.md}}

### Minimal API

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();
var hello = app.NewVersionedApi();
var v1 = hello.MapGroup( "/helloworld" ).HasApiVersion( 1.0 );
var v2_v3 = hello.MapGroup( "/helloworld" )
                 .HasApiVersion( 2.0 )
                 .HasApiVersion( 3.0 );

v1.MapGet( "/", () => "Hello world v1.0!" );
v2_v3.MapGet( "/", () => "Hello world v2.0!" ).MapToApiVersion( 2.0 );
v2_v3.MapGet( "/", () => "Hello world v3.0!" ).MapToApiVersion( 3.0 );

app.Run();
```

### MVC (Core)

```c#
[ApiVersion( 1.0 )]
[ApiController]
[Route( "api/[controller]" )]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world v1.0!";
}

[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[ApiController]
[Route( "api/helloworld" )]
public class HelloWorld2Controller : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world v2.0!";

    [HttpGet, MapToApiVersion( 3.0 )]
    public string GetV3() => "Hello world v3.0!";
}
```

### OData

```c#
[ApiVersion( 1.0 )]
public class PeopleController : ODataController
{
    public IActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );
}

[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[ControllerName( "People" )]
public class People2Controller : ODataController
{
    public IActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );

    [MapToApiVersion( 3.0 )]
    public IActionResult GetV3( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );
}
```

{{#include ../../shared/how-to/version-interleaving-post.md}}