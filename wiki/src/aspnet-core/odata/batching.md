<!-- description: Configure version-specific OData batch handlers with the API versioning batch middleware. -->

# Batching

OData batch operations are meant to execute the same way that other requests do; however, there may be some minor, but
crucial differences required in the setup configuration depending on your target platform.

OData batch operations are facilitated by the OData batching middleware in ASP.NET Core. The built-in OData middleware
only allows a single `ODataBatchHandler` and cannot be extended. In theory, there should only be a single
`ODataBatchHandler` for the entire application, but there is no guarantee that is what a developer has done or wants.
API Versioning, therefore, provides alternate OData batch middleware that allows a version-specific `ODataBatchHandler`
if that is what you have configured. Additionally, API Versioning always registers a default `ODataBatchHandler` in
`AddRouteComponents` so you don't have to, which is something OData does **not** do by default.

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers().AddOData();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning()
                .AddOData( options => options.AddRouteComponents( "api" ) );

var app = builder.Build();

app.UseVersionedODataBatching();
app.UseRouting()
app.UseEndpoints( endpoints => endpoints.MapControllers() );
app.Run();
```