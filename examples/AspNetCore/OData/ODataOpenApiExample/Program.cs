using ApiVersioning.Examples;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OData;
using Scalar.AspNetCore;
using System.Reflection;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using PeopleControllerV2 = ApiVersioning.Examples.V2.PeopleController;
using PeopleControllerV3 = ApiVersioning.Examples.V3.PeopleController;

[assembly: AssemblyDescription( "An example API" )]

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers()
                .AddOData(
                    options =>
                    {
                        options.Count().Select().OrderBy();
                        options.RouteOptions.EnableKeyInParenthesis = false;
                        options.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
                        options.RouteOptions.EnablePropertyNameCaseInsensitive = true;
                        options.RouteOptions.EnableQualifiedOperationCall = false;
                        options.RouteOptions.EnableUnqualifiedOperationCall = true;
                    } );
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning(
                    options =>
                    {
                        // reporting api versions will return the headers
                        // "api-supported-versions" and "api-deprecated-versions"
                        options.ReportApiVersions = true;

                        options.Policies.Deprecate( 0.9 )
                                        .Effective( DateTimeOffset.Now )
                                        .Link( "policy.html" )
                                            .Title( "Version Deprecation Policy" )
                                            .Type( "text/html" );

                        options.Policies.Sunset( 0.9 )
                                        .Effective( DateTimeOffset.Now.AddDays( 60 ) )
                                        .Link( "policy.html" )
                                            .Title( "Version Sunset Policy" )
                                            .Type( "text/html" );
                    } )
                .AddOData( options => options.AddRouteComponents( "api" ) )
                .AddODataApiExplorer(
                    options =>
                    {
                        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                        // note: the specified format code will format the version as "'v'major[.minor][-status]"
                        options.GroupNameFormat = "'v'VVV";

                        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                        // can also be used to control the format of the API version in route templates
                        options.SubstituteApiVersionInUrl = true;

                        // configure query options (which cannot otherwise be configured by OData conventions)
                        options.QueryOptions.Controller<PeopleControllerV2>()
                                            .Action( c => c.Get( default ) )
                                                .Allow( Skip | Count )
                                                .AllowTop( 100 )
                                                .AllowOrderBy( "firstName", "lastName" );

                        options.QueryOptions.Controller<PeopleControllerV3>()
                                            .Action( c => c.Get( default ) )
                                                .Allow( Skip | Count )
                                                .AllowTop( 100 )
                                                .AllowOrderBy( "firstName", "lastName" );
                    } )
                .AddOpenApi( options => options.Document.AddScalarTransformers() );

var app = builder.Build();

if ( app.Environment.IsDevelopment() )
{
    // access ~/$odata to identify OData endpoints that failed to match a route template
    app.UseODataRouteDebug();
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