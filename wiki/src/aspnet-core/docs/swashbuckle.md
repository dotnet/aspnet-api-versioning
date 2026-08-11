<!-- description: Wire Swashbuckle up to a versioned ASP.NET Core service with operation and document filters. -->

{{#include ../../shared/docs/swashbuckle-pre.md}}

Remember to add the necessary references to one or both of the following:

- [API Explorer Extensions for ASP.NET Core](https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer)
- [API Explorer Extensions for ASP.NET Core with OData](https://www.nuget.org/packages/Asp.Versioning.OData.ApiExplorer)

```c#
public class SwaggerDefaultValues : IOperationFilter
{
  public void Apply( OpenApiOperation operation, OperationFilterContext context )
  {
    var apiDescription = context.ApiDescription;

    operation.Deprecated |= apiDescription.IsDeprecated();

    foreach ( var responseType in context.ApiDescription.SupportedResponseTypes )
    {
        var responseKey = responseType.IsDefaultResponse
                          ? "default"
                          : responseType.StatusCode.ToString();
        var response = operation.Responses[responseKey];

        foreach ( var contentType in response.Content.Keys )
        {
            if ( !responseType.ApiResponseFormats.Any( x => x.MediaType == contentType ) )
            {
                response.Content.Remove( contentType );
            }
        }
    }

    if ( operation.Parameters == null )
    {
        return;
    }

    foreach ( var parameter in operation.Parameters )
    {
        var description = apiDescription.ParameterDescriptions
                                        .First( p => p.Name == parameter.Name );

        parameter.Description ??= description.ModelMetadata?.Description;

        if ( parameter.Schema.Default == null && description.DefaultValue != null )
        {
            var json = JsonSerializer.Serialize(
                description.DefaultValue,
                description.ModelMetadata.ModelType );
            parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson( json );
        }

        parameter.Required |= description.IsRequired;
    }
  }
}
```

We also need a way to tell Swashbuckle about the API versions in the application:

```c#
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider;

    public ConfigureSwaggerOptions( IApiVersionDescriptionProvider provider ) => this.provider = provider;

    public void Configure( SwaggerGenOptions options )
    {
        foreach ( var description in provider.ApiVersionDescriptions )
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo()
                {
                    Title = "Example API",
                    Description = "An example API",
                    Version = description.ApiVersion.ToString(),
                } );
        }
    }
}
```

Now we can put it all together:

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers();
builder.Services.AddApiVersioning().AddMvc().AddApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen( options => options.OperationFilter<SwaggerDefaultValues>() );

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(
    options =>
    {
        foreach ( var description in app.DescribeApiVersions() )
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName );
        }
    } );

app.MapControllers();
app.Run();
```

## Examples

There are end-to-end examples using API versioning and Swashbuckle:

- [Minimal APIs, API Versioning and Swashbuckle](https://github.com/dotnet/aspnet-api-versioning/tree/release/8.1/examples/AspNetCore/WebApi/MinimalOpenApiExample)
- [MVC (Core), API Versioning and Swashbuckle](https://github.com/dotnet/aspnet-api-versioning/tree/release/8.1/examples/AspNetCore/WebApi/OpenApiExample)
- [OData, API Versioning, and Swashbuckle](https://github.com/dotnet/aspnet-api-versioning/tree/release/8.1/examples/AspNetCore/OData/ODataOpenApiExample)
- [Partial OData, API Versioning, and Swashbuckle](https://github.com/dotnet/aspnet-api-versioning/tree/release/8.1/examples/AspNetCore/OData/SomeODataOpenApiExample)