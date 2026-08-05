{{#include ../../shared/how-to/overview-pre.md}}

### Minimal API

Minimal APIs do not use controllers nor any of these conventions or attributes. The intrinsic grouping capabilities
define collation without having to infer anything. It is, however, possible to add a logical API name to the group if
you want to:

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();
var people = app.NewVersionedApi( "People" ); // ← provides optional, logical name

people.MapGet( "/people", () => new[] { new Person() } ).HasApiVersion( 1.0 );

app.Run();
```

{{#include ../../shared/how-to/overview-post.md}}