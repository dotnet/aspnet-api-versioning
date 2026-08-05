{{#include ../../shared/how-to/requested-version-pre.md}}

### Minimal API

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning().EnableApiVersionBinding();

var app = builder.Build();
var api = app.NewVersionedApi();

api.MapGet( "/", ( ApiVersion version ) => Results.Ok() )
   .HasApiVersion( 1.0 )
   .HasApiVersion( 2.0 );

app.Run();
```

### MVC (Core)

```c#
[ApiVersion( 1.0 )]
[ApiVersion( 2.0 )]
[ApiController]
public class Controller : ControllerBase
{
    public IActionResult Get()
    {
        var apiVersion = HttpContext.RequestedApiVersion;
        return Ok();
    }

    // supported in 3.0+
    public IActionResult Get( int id, ApiVersion apiVersion ) => Ok();
}
```