using Microsoft.AspNetCore.OData;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

builder.Services.AddControllers().AddOData();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning()
                .AddOData(
                    options =>
                    {
                        // INFO: you do NOT and should NOT use both the query
                        // string and url segment methods together. this configuration
                        // is merely illustrating that they can coexist and allows you
                        // to easily experiment with either configuration. one of these
                        // would be removed in a real application.
                        
                        // WHEN VERSIONING BY: query string, header, or media type
                        options.AddRouteComponents( "api" );
                        
                        // WHEN VERSIONING BY: url segment
                        options.AddRouteComponents( "api/v{version:apiVersion}" );
                    } );

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();