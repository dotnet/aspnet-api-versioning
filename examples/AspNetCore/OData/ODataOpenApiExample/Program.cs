using ApiVersioning.Examples;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using PeopleControllerV2 = ApiVersioning.Examples.V2.PeopleController;
using PeopleControllerV3 = ApiVersioning.Examples.V3.PeopleController;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

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

                        options.Policies.Sunset( 0.9 )
                                        .Effective( DateTimeOffset.Now.AddDays( 60 ) )
                                        .Link( "policy.html" )
                                            .Title( "Versioning Policy" )
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
                    } );

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(
    options =>
    {
        // add a custom operation filter which sets default values
        options.OperationFilter<SwaggerDefaultValues>();

        var fileName = typeof( Program ).Assembly.GetName().Name + ".xml";
        var filePath = Path.Combine( AppContext.BaseDirectory, fileName );

        // integrate xml comments
        options.IncludeXmlComments( filePath );
    } );

var app = builder.Build();

// Configure HTTP request pipeline.

if ( app.Environment.IsDevelopment() )
{
    // Access ~/$odata to identify OData endpoints that failed to match a route template.
    app.UseODataRouteDebug();
}

app.UseSwagger();
if ( app.Environment.IsDevelopment() )
{
app.UseSwaggerUI(
    options =>
    {
        var descriptions = app.DescribeApiVersions();

        // build a swagger endpoint for each discovered API version
        foreach ( var description in descriptions )
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint( url, name );
        }
    } );
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();