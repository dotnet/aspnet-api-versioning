<!-- description: Version an ASP.NET Core OData service with an Entity Data Model per API version. -->

{{#include ../../shared/odata/overview-pre.md}}

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers().AddOData();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning()
                .AddOData( options => options.AddRouteComponents( "api" ) );

var app = builder.Build();

app.MapControllers();
app.Run();
 ```

It is possible to imperatively use:

```c#
.AddOData( options => options.ModelConfigurations.Add( new PersonModelConfiguration() ) )
```

however, it is typically unnecessary because this will automatically happen via dependency injection.

>[!IMPORTANT]
>Calling `AddControllers().AddOData( options => options.AddRouteComponents( ... ) )` will be completely ignored by API
>Versioning. Due to the OData design, it is impossible to extend or customize this behavior. Instead, you need to use
>`AddApiVersioning().AddOData( options => options.AddRouteComponents( ... ) )`. The standard `AddOData` configuration
>can still be used to configure global query option settings.
