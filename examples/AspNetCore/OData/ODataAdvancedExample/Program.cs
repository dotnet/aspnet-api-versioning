using Asp.Versioning;
using Microsoft.AspNetCore.OData;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

builder.Services.AddControllers().AddOData();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning(
                    options =>
                    {
                        // reporting api versions will return the headers
                        // "api-supported-versions" and "api-deprecated-versions"
                        options.ReportApiVersions = true;

                        // allows a client to make a request without specifying an
                        // api version. the value of options.DefaultApiVersion will
                        // be 'assumed'; this is meant to grandfather in legacy apis
                        options.AssumeDefaultVersionWhenUnspecified = true;

                        // allow multiple locations to request an api version
                        options.ApiVersionReader = ApiVersionReader.Combine(
                            new QueryStringApiVersionReader(),
                            new HeaderApiVersionReader( "api-version", "x-ms-version" ) );
                    } )
                .AddOData( options => options.AddRouteComponents( "api" ) );

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();