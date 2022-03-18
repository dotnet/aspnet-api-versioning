using Asp.Versioning.Conventions;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.
builder.Services.AddApiVersioning();

var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.DefineApi()
   .HasApiVersion( 1.0 )
   .HasApiVersion( 2.0 )
   .ReportApiVersions()
   .HasMapping( api =>
    {
        // GET /weatherforecast?api-version=1.0
        api.MapGet( "/weatherforecast", () =>
            {
                return Enumerable.Range( 1, 5 ).Select( index =>
                    new WeatherForecast
                    (
                        DateTime.Now.AddDays( index ),
                        Random.Shared.Next( -20, 55 ),
                        summaries[Random.Shared.Next( summaries.Length )]
                    ) );
            } )
           .MapToApiVersion( 1.0 );

        // GET /weatherforecast?api-version=2.0
        api.MapGet( "/weatherforecast", () =>
            {
                return Enumerable.Range( 0, summaries.Length ).Select( index =>
                    new WeatherForecast
                    (
                        DateTime.Now.AddDays( index ),
                        Random.Shared.Next( -20, 55 ),
                        summaries[Random.Shared.Next( summaries.Length )]
                    ) );
            } )
           .MapToApiVersion( 2.0 );

        // POST /weatherforecast?api-version=2.0
        api.MapPost( "/weatherforecast", ( WeatherForecast forecast ) => { } )
           .MapToApiVersion( 2.0 );

        // DELETE /weatherforecast
        api.MapDelete( "/weatherforecast", () => { } )
           .IsApiVersionNeutral();
    } );

app.Run();

internal record WeatherForecast( DateTime Date, int TemperatureC, string? Summary )
{
    public int TemperatureF => 32 + (int) ( TemperatureC / 0.5556 );
}