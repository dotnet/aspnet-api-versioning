<!-- description: Version an ASP.NET Core API with a query string parameter. -->

{{#include ../../shared/how-to/version-by-query-string-pre.md}}

### Minimal API

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();
var hello = app.NewVersionedApi();
var v1 = hello.MapGroup( "/helloworld" ).HasApiVersion( 1.0 );
var v2 = hello.MapGroup( "/helloworld" ).HasApiVersion( 2.0 );

v1.MapGet( "/", () => "Hello world!" );
v2.MapGet( "/", () => "Hello world!" );

app.Run();
```

### MVC (Core)

```c#
[ApiController]
[Route( "api/[controller]" )]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world!";
}
```

### OData

```c#
public class PeopleController : ODataController
{
    [HttpGet]
    public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );
}
```

### Next Version

To create the next version of the controller, you can choose to create a new controller with the same route but
decorate it as API version `2.0`. For example:

#### MVC (Core)

```c#
[ApiVersion( 2.0 )]
[ApiController]
[Route( "api/helloworld" )]
public class HelloWorld2Controller : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world!";
}
```

#### OData

```c#
[ApiVersion( 2.0 )]
[ControllerName( "People" )]
public class People2Controller : ODataController
{
    [HttpGet]
    public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );
}
```

{{#include ../../shared/how-to/version-by-query-string-post.md}}