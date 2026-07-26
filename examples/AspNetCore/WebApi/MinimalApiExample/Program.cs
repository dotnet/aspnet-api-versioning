using Asp.Versioning;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

builder.Services.AddProblemDetails();

// enable api versioning and return the headers
// "api-supported-versions" and "api-deprecated-versions"
builder.Services.AddApiVersioning( options => options.ReportApiVersions = true );

var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var forecast = app.NewVersionedApi();

// GET /weatherforecast?api-version=1.0
forecast.MapGet( "/weatherforecast", () =>
         {
             return Enumerable.Range( 1, 5 ).Select( index =>
                 new WeatherForecast
                 (
                     DateTime.Now.AddDays( index ),
                     Random.Shared.Next( -20, 55 ),
                     summaries[Random.Shared.Next( summaries.Length )]
                 ) );
         } )
        .HasApiVersion( 1.0 );

// GET /weatherforecast?api-version=2.0
var v2 = forecast.MapGroup( "/weatherforecast" )
                 .HasApiVersion( 2.0 );

v2.MapGet( "/", ( ApiVersion version ) =>
   {
       return Enumerable.Range( 0, summaries.Length ).Select( index =>
           new WeatherForecast
           (
               DateTime.Now.AddDays( index ),
               Random.Shared.Next( -20, 55 ),
               summaries[Random.Shared.Next( summaries.Length )]
           ) );
   } );

// POST /weatherforecast?api-version=2.0
v2.MapPost( "/", ( WeatherForecast forecast ) => Results.Ok() );

// DELETE /weatherforecast
forecast.MapDelete( "/weatherforecast", () => Results.NoContent() )
        .IsApiVersionNeutral();

// ---- IntroducedInApiVersion demonstration ----
//
// An explicit api version set declares v1.0, v2.0, and v3.0. Endpoints
// attached to it inherit that set, so IntroducedInApiVersion's
// "from-this-version-onward" expansion has the full controller-declared
// set to filter against.

var multiVersionSet = app.NewApiVersionSet( "MultiVersioned" )
                         .HasApiVersion( new ApiVersion( 1.0 ) )
                         .HasApiVersion( new ApiVersion( 2.0 ) )
                         .HasApiVersion( new ApiVersion( 3.0 ) )
                         .Build();

// .HasApiVersion( 2.0 ) on an endpoint attached to a version set that
// also declares v1.0 and v3.0 is exact-match — equivalent to
// [MapToApiVersion(2.0)]. v1.0 and v3.0 callers receive the configured
// UnsupportedApiVersionStatusCode (default 400).
app.MapGet( "/multiversioned/legacy", ( ApiVersion version ) =>
            Results.Ok( $"Legacy {version}" ) )
   .WithApiVersionSet( multiVersionSet )
   .HasApiVersion( 2.0 );

// .IntroducedInApiVersion( 2.0 ) is "from this version onward against
// the declared set." Reachable for v2.0 AND v3.0 automatically.
// Requests under v1.0 receive the per-attribute status (default 404).
// When v4.0 is added to multiVersionSet, this endpoint becomes reachable
// for v4.0 with no further changes.
app.MapGet( "/multiversioned/modern", ( ApiVersion version ) =>
            Results.Ok( $"Modern {version}" ) )
   .WithApiVersionSet( multiVersionSet )
   .IntroducedInApiVersion( 2.0 );

app.Run();

internal record WeatherForecast( DateTime Date, int TemperatureC, string? Summary )
{
    public int TemperatureF => 32 + (int) ( TemperatureC / 0.5556 );
}