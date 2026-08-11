<!-- description: Set up API versioning in a new ASP.NET Core service using minimal APIs, MVC, gRPC, or OData. -->

{{#include ../../shared/quick-starts/new-services.md}}

### Minimal API

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();
var people = app.NewVersionedApi();

people.MapGet( "/people", () => new[] { new Person() } ).HasApiVersion( 1.0 );

app.Run();
```

### MVC (Core)

```c#
[ApiVersion( 1.0 )]
[ApiController]
[Route( "[controller]" )]
public class PeopleController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok( new[] { new Person() } );
}
```

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning().AddMvc();

var app = builder.Build();

app.MapControllers();
app.Run();
```

### OData

```c#
[ApiVersion( 1.0 )]
public class PeopleController : ODataController
{
    [EnableQuery]
    public IActionResult Get() => Ok( new[] { new Person() } );
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
        options.AddRouteComponents();
    } );

var app = builder.Build();

app.MapControllers();
app.Run();
```