using ApiVersioning.Examples.Controllers;
using Asp.Versioning.Conventions;

[assembly: Microsoft.AspNetCore.Mvc.ApiController]

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning(
                    options =>
                    {
                        // reporting api versions will return the headers
                        // "api-supported-versions" and "api-deprecated-versions"
                        options.ReportApiVersions = true;

                    } )
                .AddMvc(
                    options =>
                    {
                        options.Conventions.Controller<ValuesController>().HasApiVersion( 1.0 );

                        options.Conventions.Controller<Values2Controller>()
                                           .HasApiVersion( 2.0 )
                                           .HasApiVersion( 3.0 )
                                           .Action( c => c.GetV3( default ) ).MapToApiVersion( 3.0 )
                                           .Action( c => c.GetV3( default, default ) ).MapToApiVersion( 3.0 );

                        options.Conventions.Controller<HelloWorldController>()
                                           .HasApiVersion( 1.0 )
                                           .HasApiVersion( 2.0 )
                                           .AdvertisesApiVersion( 3.0 );
                    } );

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();