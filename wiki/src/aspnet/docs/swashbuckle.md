<!-- description: Wire Swashbuckle up to a versioned ASP.NET Web API service with operation and document filters. -->

{{#include ../../shared/docs/swashbuckle-pre.md}}

Remember to add the necessary references to one or both of the following:

- [API Explorer Extensions for ASP.NET Web API](https://www.nuget.org/packages/Asp.Versioning.WebApi.ApiExplorer)
- [API Explorer Extensions for ASP.NET Web API with OData](https://www.nuget.org/packages/Asp.Versioning.WebApi.OData.ApiExplorer)

```c#
public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(
        Operation operation,
        SchemaRegistry schemaRegistry,
        ApiDescription apiDescription )
    {
        operation.deprecated |= apiDescription.IsDeprecated();

        if ( operation.parameters == null )
        {
            return;
        }

        foreach ( var parameter in operation.parameters )
        {
            var description = apiDescription.ParameterDescriptions
                                            .First( p => p.Name == parameter.name );

            parameter.description ??= description.Documentation;
            parameter.@default ??= description.ParameterDescriptor?.DefaultValue;
        }
    }
}
```

Use `MultipleApiVersions` to iterate over each `ApiDescription` and collate them by their corresponding group. The
default group name for each `ApiDescription` is the formatted API version that is associated with it.

```c#
configuration.EnableSwagger(
    "{apiVersion}/swagger",
    swagger =>
    {
        swagger.MultipleApiVersions(
            ( apiDescription, version ) => apiDescription.GetGroupName() == version,
            info =>
            {
                foreach ( var group in apiExplorer.ApiDescriptions )
                {
                    info.Version( group.Name, $"Example API {group.ApiVersion}" )
                        .Description( "An example API" );
                }
            } );
        swagger.OperationFilter<SwaggerDefaultValues>();
    } )
    .EnableSwaggerUi( swagger => swagger.EnableDiscoveryUrlSelector() );
```

### Examples

There are end-to-end examples using API versioning and Swashbuckle:

- [API Versioning and Swashbuckle](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNet/WebApi/OpenApiWebApiExample)
- [OData, API Versioning, and Swashbuckle](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNet/OData/OpenApiODataWebApiExample)
- [Partial OData, API Versioning, and Swashbuckle](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNet/OData/SomeOpenApiODataWebApiExample)