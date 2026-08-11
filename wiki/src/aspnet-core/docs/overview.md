<!-- description: Document a versioned ASP.NET Core service with OpenAPI using the versioned API explorers. -->

{{#include ../../shared/docs/overview-pre.md}}

Any OpenAPI generator such as [Microsoft][openapi-ms], [Swashbuckle][openapi-swashbuckle], or [NSwag][openapi-nswag]
that leverage the API Explorer can be used.

## Minimal API or MVC (Core)

[![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.Mvc.ApiExplorer.svg)](https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer) [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.OpenApi.svg)](https://www.nuget.org/packages/Asp.Versioning.OpenApi)

>[!NOTE]
>Applies to ASP.NET Core 10.0+. For earlier versions, see the [previous examples] with
[Swashbuckle][openapi-swashbuckle].

Everything you need to add versioned documentation to your Minimal and controller-based APIs using the
[API Explorer extensions][explorer-mvc], [OpenAPI extensions], and [Scalar].

```c#
var builder = WebApplication.CreateBuilder( args );

// only required if you're using controllers
builder.Services.AddControllers();

builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning()
                .AddMvc() // ← bring in MVC (Core); not required for Minimal APIs
                .AddApiExplorer(
                     // (optional) format the version as "'v'major[.minor][-status]"
                     options => options.GroupNameFormat = "'v'VVV" )
                .AddOpenApi(
                     // (optional) apply Scalar-specific transformers
                     options => options.Document.AddScalarTransformers()
                );

var app = builder.Build();

// configure OpenAPI and Scalar to use a document per version
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

// only required if you're using controllers
app.MapControllers();

app.Run();
```

Review the following example projects for additional setup and configuration options:

- [OpenAPI Example](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/WebApi/OpenApiExample)
- [Minimal OpenAPI Example](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/WebApi/MinimalOpenApiExample)

## gRPC

[![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.Grpc.svg)](https://www.nuget.org/packages/Asp.Versioning.Grpc) [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.Grpc.ApiExplorer.svg)](https://www.nuget.org/packages/Asp.Versioning.Grpc.ApiExplorer) [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.OpenApi.svg)](https://www.nuget.org/packages/Asp.Versioning.OpenApi)

Everything you need to add versioned documentation to your gRPC APIs using the [API Explorer extensions],
[OpenAPI extensions], and [Scalar].

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddApiVersioning()
                .AddGrpc()
                .AddApiExplorer(
                     // (optional) format the version as "'v'major[.minor][-status]"
                     options => options.GroupNameFormat = "'v'VVV" )
                .AddGrpcApiExplorer()
                .AddOpenApi(
                     // (optional) apply Scalar-specific transformers
                     options => options.Document.AddScalarTransformers()
                );

var app = builder.Build();
var greeter = app.NewVersionedApi( "Greeter" );

greeter.MapGrpcService<GreeterService>()
       .HasApiVersion( 1.0 )
       .HasApiVersion( 2.0 )
       .HasApiVersion( 3.0 );

// configure OpenAPI and Scalar to use a document per version
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

app.Run();
```

Review the following example projects for additional setup and configuration options:

- [gRPC OpenAPI Example](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/WebApi/GrpcOpenApiExample)

## OData

[![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.OData.ApiExplorer.svg)](https://www.nuget.org/packages/Asp.Versioning.OData.ApiExplorer) [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.OpenApi.svg)](https://www.nuget.org/packages/Asp.Versioning.OpenApi)

Everything you need to add versioned documentation to your OData controllers using the
[API Explorer extensions][explorer-odata], [OpenAPI extensions], and [Scalar].

```c#
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers().AddOData();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning()
                .AddOData( options => options.AddRouteComponents() )
                .AddODataApiExplorer(
                     // (optional) format the version as "'v'major[.minor][-status]"
                     options => options.GroupNameFormat = "'v'VVV" )
                .AddOpenApi(
                     // (optional) apply Scalar-specific transformers
                     options => options.Document.AddScalarTransformers()
                );

var app = builder.Build();

// configure OpenAPI and Scalar to use a document per version
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

app.MapControllers();
app.Run();
```

Review the following example projects for additional setup and configuration options:

- [OData OpenAPI Example](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/OData/ODataOpenApiExample)
- [Partial OData OpenAPI Example](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/OData/SomeODataOpenApiExample)

[previous examples]: https://github.com/dotnet/aspnet-api-versioning/tree/release/8.1/examples/AspNetCore/WebApi
[explorer-mvc]: https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer
[explorer-odata]: https://www.nuget.org/packages/Asp.Versioning.OData.ApiExplorer
[OpenAPI extensions]: https://www.nuget.org/packages/Asp.Versioning.OpenApi

[openapi-ms]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview
[openapi-swashbuckle]: https://github.com/domaindrivendev/Swashbuckle.AspNetCore
[openapi-nswag]: https://github.com/RicoSuter/NSwag
[Scalar]: https://scalar.com/