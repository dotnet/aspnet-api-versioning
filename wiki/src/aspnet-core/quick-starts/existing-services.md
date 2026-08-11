<!-- description: Retrofit API versioning onto an ASP.NET Core service already in production without breaking clients. -->

{{#include ../../shared/quick-starts/existing-services.md}}

### Minimal API

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddProblemDetails();

// allow a client to call you without specifying an api version
// since we haven't configured it otherwise, the assumed api version will be 1.0
builder.Services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true );
        
var app = builder.Build();
var people = app.NewVersionedApi();
var v1 = people.MapGroup( "/people" ).HasApiVersion( 1.0 );
var v2 = people.MapGroup( "/people" ).HasApiVersion( 2.0 );

v1.MapGet( "/", () => new[] { new Person() } );
v2.MapGet( "/", () => new[] { new Person() } );

app.Run();
```

### MVC (Core)

```c#
[ApiVersion( 1.0 )] // ← this attribute isn't required, but it's easier to understand
[ApiController]
[Route( "[controller]" )]
public class PeopleController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok( new[] { new Person() } );
}

[ApiVersion( 2.0 )]
[ApiController]
[Route( "People" )]
public class People2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok( new[] { new Person() } );
}
```

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// allow a client to call you without specifying an api version
// since we haven't configured it otherwise, the assumed api version will be 1.0
builder.Services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true )
                .AddMvc();
        
var app = builder.Build();

app.MapController();
app.Run();
```

### OData

```c#
[ApiVersion( 1.0 )] // ← this attribute isn't required, but it's easier to understand
public class PeopleController : ODataController
{
    // GET ~/people
    // GET ~/people?api-version=1.0
    [EnableQuery]
    public IActionResult Get() => Ok( new[] { new Person() } );
}

[ApiVersion( 2.0 )]
[ControllerName( "People" )]
public class People2Controller : ODataController
{
    // GET ~/people?api-version=2.0
    [EnableQuery]
    public IActionResult Get() => Ok( new[] { new Person() } );
}
```

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers().AddOData();
builder.Services.AddProblemDetails();

// allow a client to call you without specifying an api version
// since we haven't configured it otherwise, the assumed api version will be 1.0
builder.Services
       .AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true )
       .AddOData( options =>
        {
            options.ModelBuilder.DefaultModelConfiguration = ( builder, apiVersion, routePrefix ) =>
            {
                builder.EntitySet<Person>( "People" );
            };
            options.AddRouteComponents();
        } );

var app = builder.Build();

app.MapController();
app.Run();
```