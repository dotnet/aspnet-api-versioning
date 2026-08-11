<!-- description: The GrpcApiExplorerOptions settings, including the route parameter that carries the API version. -->

# gRPC Options

The API Explorer support for gRPC has a few options that allow you to customize the behavior. The options are minimal
because the API Explorer support can technically operate without API Versioning. Most of the configuration will be
performed against the versioned API Explorer, but there are a few options that are specific to gRPC.

The `GrpcApiExplorerOptions` have the following configuration settings:

- [RouteParameter](#route-parameter)

## Route Parameter

Represents route parameter information required for API exploration.

### Name

When versioning by URL path segment, there is not a clear way to identify which segment represents the API version.
Rather than use an explicit annotation, the route parameter is matched by name. The default value of the `Name`
property is `"api_version"`; however, you can use whatever name you choose. The name will be matched in a
case-sensitive manner.

## Prefix Literal

gRPC supports route parameters in route templates, but a parameter must match an entire segment. It cannot match part
of a segment in the same manner as an ASP.NET route constraint and an API version does not include literal characters
such as `'v'`. As a result, the character is not included in the route template.

The `PrefixLiteral` property adds the expected literal in the route template when it is built for the API Explorer. As
an example, the gRPC route template `"api/{api-version}/example"` will be generated as `"api/v{api-version}/example"`
and produce the expected behavior in the API Explorer.