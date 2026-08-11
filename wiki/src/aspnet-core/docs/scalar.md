<!-- description: Point the Scalar OpenAPI user interface at the documents generated for each API version. -->

# Scalar Integration

[Scalar](https://scalar.com/) has quickly become one of the more common, modern OpenAPI user interfaces and it easily
integrates with API Versioning. The only thing you need to do is tell Scalar about the documents your application will
generate.

Remember to add the necessary references to one or both of the following:

- [Versioned OpenAPI Extensions for ASP.NET Core](https://www.nuget.org/packages/Asp.Versioning.OpenApi)
- [API Explorer Extensions for ASP.NET Core](https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer)
- [API Explorer Extensions for ASP.NET Core with gRPC](https://www.nuget.org/packages/Asp.Versioning.Grpc.ApiExplorer)
- [API Explorer Extensions for ASP.NET Core with OData](https://www.nuget.org/packages/Asp.Versioning.OData.ApiExplorer)
- [Scalar for ASP.NET Core](https://www.nuget.org/packages/Scalar.AspNetCore)
- [Scalar for ASP.NET Core with Microsoft OpenAPI extensions](https://www.nuget.org/packages/Scalar.AspNetCore.Microsoft)

**Minimal APIs**

```c#
builder.Services.AddApiVersioning()
                .AddApiExplorer()
                .AddOpenApi( options => options.Document.AddScalarTransformers() );
```

**Controllers**

```c#
builder.Services.AddApiVersioning()
                .AddMvc()
                .AddApiExplorer()
                .AddOpenApi( options => options.Document.AddScalarTransformers() );
```

**gRPC**

```c#
builder.Services.AddApiVersioning()
                .AddApiExplorer()
                .AddGrpc()
                .AddGrpcApiExplorer()
                .AddOpenApi( options => options.Document.AddScalarTransformers() );
```

**OData**

```c#
builder.Services.AddApiVersioning()
                .AddOData()
                .AddODataApiExplorer()
                .AddOpenApi( options => options.Document.AddScalarTransformers() );
```

Once you have that configured, you need only generate an OpenAPI document per version and let Scalar know which
generated documents it should expect.

```c#
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
```

## Examples

There are end-to-end examples using API versioning, OpenAPI, and Scalar:

- [Minimal APIs, API Versioning and Scalar](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/WebApi/MinimalOpenApiExample)
- [MVC (Core), API Versioning and Scalar](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/WebApi/OpenApiExample)
- [gRPC, API Versioning and Scalar](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/WebApi/GrpcOpenApiExample)
- [OData, API Versioning, and Scalar](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/OData/ODataOpenApiExample)
- [Partial OData, API Versioning, and Scalar](https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNetCore/OData/SomeODataOpenApiExample)