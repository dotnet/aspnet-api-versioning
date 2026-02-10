using Asp.Versioning;
using Scalar.AspNetCore;
using ApiVersioning.Examples.Services;
using System.Reflection;

[assembly: AssemblyDescription( "An example API" )]

var builder = WebApplication.CreateBuilder( args );
var services = builder.Services;

services.AddProblemDetails();
services.AddEndpointsApiExplorer();
services.AddApiVersioning(
            options =>
            {
                // reporting api versions will return the headers
                // "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;

                options.Policies.Sunset( 0.9 )
                                .Effective( DateTimeOffset.Now.AddDays( 60 ) )
                                .Link( "policy.html" )
                                    .Title( "Versioning Policy" )
                                    .Type( "text/html" );
            } )
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
        .AddOpenApi( options => options.Document.AddScalarTransformers() )
        // this enables binding ApiVersion as a endpoint callback parameter. if you don't use it, then
        // you should remove this configuration.
        .EnableApiVersionBinding();

var app = builder.Build();

app.MapOrders().ToV1().ToV2().ToV3();
app.MapPeople().ToV1().ToV2().ToV3();

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

app.Run();