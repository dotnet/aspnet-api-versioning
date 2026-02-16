using ApiVersioning.Examples;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Scalar.AspNetCore;
using System.Reflection;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using static System.Text.Json.JsonNamingPolicy;

[assembly: AssemblyDescription( "An example API" )]

var builder = WebApplication.CreateBuilder( args );

// note: this example application intentionally only illustrates the
// bare minimum configuration for OpenAPI with partial OData support.
// see the OpenAPI or OData OpenAPI examples for additional options.

builder.Services.Configure<JsonOptions>(
    options =>
    {
        // odata projection operations (ex: $select) use a dictionary, but for good
        // measure set the default property naming policy for any other use cases
        options.JsonSerializerOptions.PropertyNamingPolicy = CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = CamelCase;
    } );

builder.Services.AddControllers()
                .AddOData( options => options.Select() );
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning()
                .AddODataApiExplorer(
                    options =>
                    {
                        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                        // note: the specified format code will format the version as "'v'major[.minor][-status]"
                        options.GroupNameFormat = "'v'VVV";

                        // configure query options (which cannot otherwise be configured by OData conventions)
                        options.QueryOptions.Controller<BooksController>()
                                            .Action( c => c.Get( default ) )
                                                .Allow( Skip | Count )
                                                .AllowTop( 100 )
                                                .AllowOrderBy( "title", "published" );
                    } )
                .AddOpenApi( options => options.Document.AddScalarTransformers() );

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();