<!-- description: Document a versioned ASP.NET Web API service with Swagger using the versioned API explorers. -->

{{#include ../../shared/docs/overview-pre.md}}

Any OpenAPI generator such as [Swashbuckle][openapi-swashbuckle], or [NSwag][openapi-nswag] that leverage the API
Explorer can be used.

## Web API

[![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.WebApi.svg)](https://www.nuget.org/packages/Asp.Versioning.WebApi)

Everything you need to add versioned documentation to your API controllers using [API Explorer extensions](https://www.nuget.org/packages/Asp.Versioning.WebApi.ApiExplorer) with [Swashbuckle][openapi-swashbuckle-old].

```c#
config.AddApiVersioning();

// (optional) format the version as "'v'major[.minor][-status]"
var apiExplorer = config.AddVersionedApiExplorer( o => o.GroupNameFormat = "'v'VVV" );

config.EnableSwagger(
    "{apiVersion}/swagger",
    swagger =>
    {
        swagger.MultipleApiVersions(
            ( apiDescription, version ) => apiDescription.GetGroupName() == version,
            info =>
            {
                foreach ( var group in apiExplorer.ApiDescriptions )
                {
                    info.Version( group.Name, $"Example API {group.ApiVersion}" );
                }
            } );
    } )
 .EnableSwaggerUi( swagger => swagger.EnableDiscoveryUrlSelector() );
```

Review the [example](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNet/WebApi/OpenApiWebApiExample) project for additional setup and configuration options.

## OData

[![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.WebApi.OData.svg)](https://www.nuget.org/packages/Asp.Versioning.WebApi.OData)

Everything you need to add versioned documentation to your OData controllers using the
[OData API Explorer extensions][explorer-odata] with [Swashbuckle][openapi-swashbuckle].

```c#
configuration.AddApiVersioning();

var modelBuilder = new VersionedODataModelBuilder( configuration )
{
    ModelConfigurations = { new MyModelConfiguration() }
};

configuration.MapVersionedODataRoutes( "odata", "api", modelBuilder );

// (optional) format the version as "'v'major[.minor][-status]"
var apiExplorer = configuration.AddODataApiExplorer( o => o.GroupNameFormat = "'v'VVV" );

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
                    info.Version( group.Name, $"Example API {group.ApiVersion}" );
                }
            } );
    } )
 .EnableSwaggerUi( swagger => swagger.EnableDiscoveryUrlSelector() );
```

Review the following example projects for additional setup and configuration options:

- [OData OpenAPI Example](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNet/OData/OpenApiODataWebApiExample)
- [Partial OData OpenAPI Example](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNet/OData/SomeOpenApiODataWebApiExample)

>[!NOTE]
>This API explorer does not directly tie into [Swashbuckle with OData](https://github.com/rbeauchamp/Swashbuckle.OData)
>because that project also prescribes how API versioning is performed, which is incompatible with this project.

[openapi-swashbuckle]: https://github.com/domaindrivendev/Swashbuckle.WebApi
[openapi-nswag]: https://github.com/RicoSuter/NSwag
[explorer-odata]: https://www.nuget.org/packages/Asp.Versioning.OData.ApiExplorer