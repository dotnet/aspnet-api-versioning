using Scalar.AspNetCore;
using System.Reflection;
using V1 = ApiVersioning.Examples.Services.V1;
using V3 = ApiVersioning.Examples.Services.V3;

[assembly: AssemblyDescription( "An example API" )]

var builder = WebApplication.CreateBuilder( args );
var services = builder.Services;

// only required because controllers are mixed in
services.AddControllers();
services.AddProblemDetails();

services.AddApiVersioning()
        .AddMvc() // only required because controllers are mixed in
        .AddApiExplorer(
            options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            } )
        .AddGrpc()
        .AddGrpcApiExplorer()
        .AddOpenApi( options => options.Document.AddScalarTransformers() );

var app = builder.Build();
var orders = app.NewVersionedApi( "Orders" );
var people = app.NewVersionedApi( "People" );
var greeter = app.NewVersionedApi( "Greeter" );

// single implementation versioned by query string, but with different transcoded fields
orders.MapGrpcService<V1.OrdersService>()
      .HasApiVersion( 1.0 )
      .HasApiVersion( 2.0 )
      .HasApiVersion( 3.0 );

// split implementations, where 2.0 is a normal controller
greeter.MapGrpcService<V1.GreeterService>().HasApiVersion( 1.0 );
greeter.MapGrpcService<V3.GreeterService>().HasApiVersion( 3.0 );

// single implementation versioned by url, but with different transcoded fields
people.MapGrpcService<V3.PeopleService>()
      .HasApiVersion( 1.0 )
      .HasApiVersion( 2.0 )
      .HasApiVersion( 3.0 );

if ( app.Environment.IsDevelopment() )
{
    app.MapOpenApi().WithDocumentPerVersion();
    app.MapScalarApiReference(
        options =>
        {
            var descriptions = app.DescribeApiVersions();

            for ( var i = 0; i < descriptions.Count; i++ )
            {
                var description = descriptions[i];
                var isDefault = i == descriptions.Count - 1;

                options.AddDocument( description.GroupName, description.GroupName, isDefault: isDefault );
            }
        } );
}

// only required because controllers are mixed in
app.MapControllers();

app.Run();