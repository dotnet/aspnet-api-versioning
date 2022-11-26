using ApiVersioning.Examples;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;
using static System.Text.Json.JsonNamingPolicy;

var builder = WebApplication.CreateBuilder( args );

// Add services to the container.

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

// Configure the HTTP request pipeline.

app.UseSwagger();
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();