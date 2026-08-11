<!-- description: Version an ASP.NET Core API by URL path segment. -->

{{#include ../../shared/how-to/version-by-url-pre.md}}

### Minimal API

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();
var people = app.NewVersionedApi();
var v1 = people.MapGroup( "/people/v{version:apiVersion}" ).HasApiVersion( 1.0 );

v1.MapGet( "/", () => new[] { new Person() } );

app.Run();
```

### MVC (Core)

```c#
[ApiVersion( 1.0 )]
[ApiController]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world!";
}

[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[ApiController]
[Route( "api/v{version:apiVersion}/helloworld" )]
public class HelloWorld2Controller : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world v2!";

    [HttpGet, MapToApiVersion( 3.0 )]
    public string GetV3() => "Hello world v3!";
}
```

### OData

```c#
[ApiVersion( 1.0 )]
public class PeopleController : ODataController
{
    [EnableQuery]
    public IQueryable<Person> Get() => new[]{ new Person() }.AsQueryable();
}

[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[ControllerName( "People" )]
public class People2Controller : ODataController
{
    [EnableQuery]
    [ODataRoute]
    public IQueryable<Person> Get() => new[]{ new Person() }.AsQueryable();

    [EnableQuery]
    [ODataRoute, MapToApiVersion( 3.0 )]
    public IQueryable<Person> GetV3() => new[]{ new Person() }.AsQueryable();
}
```

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers().AddOData();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning().AddOData(
    options =>
    {
        options.ModelBuilder.DefaultModelConfiguration = ( builder, apiVersion, routePrefix ) =>
        {
            builder.EntitySet<Person>( "People" );
        };
        options.AddRouteComponents( "api/v{version:apiVersion}" );
    } );

var app = builder.Build();

app.MapControllers();
app.Run();
```

{{#include ../../shared/how-to/version-by-url-post.md}}