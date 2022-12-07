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

v2.MapGet( "/", () =>
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

app.Run();

internal record WeatherForecast( DateTime Date, int TemperatureC, string? Summary )
{
    public int TemperatureF => 32 + (int) ( TemperatureC / 0.5556 );
}