# Introduction

Versioning is an important aspect of any mature web service. Microsoft has published REST API guidelines that require
that all compliant services must support explicit versioning. This ensures that clients can rely on services to be
stable over time, while still enabling service changes and new features. The goal of the ASP.NET API Versioning project
is to adhere to the [Microsoft REST Guidelines for versioning] using the ASP.NET technology stack out-of-the-box, but
there are numerous extensions and customizations that allow you to version your APIs however you like. Detailed
information about the recommended guidance can be found in the [Microsoft REST Guidelines].

## Features

### .NET

#### [Abstractions](https://www.nuget.org/packages/Asp.Versioning.Abstractions)

The core abstractions provide a common set of interfaces and types for API versioning across all supported platforms.
These capabilities can be used to version your data models or using version metadata outside of ASP.NET.

#### [Client](https://www.nuget.org/packages/Asp.Versioning.Client)

The client-side extensions make it simple to create API version-aware HTTP clients.

### ASP.NET Core

#### [Minimal API](https://www.nuget.org/packages/Asp.Versioning.Http)

Everything you need to add service API versioning to your ASP.NET Core applications and Minimal APIs. The
[API Explorer][explorer] and [OpenAPI][openapi] extensions provided everything you need to document your services.

#### [MVC (Core)](https://www.nuget.org/packages/Asp.Versioning.Mvc)

Expands upon the service API versioning for ASP.NET Core and adds support for controller classes. The
[API Explorer][explorer] and [OpenAPI][openapi] extensions provided everything you need to document your services.

#### [gRPC](https://www.nuget.org/packages/Asp.Versioning.Grpc)

Expands upon the service API versioning for ASP.NET Core and adds support for gRPC services. The
[API Explorer][explorer-grpc] and [OpenAPI][openapi] extensions provided everything you need to document your services.

#### [OData](https://www.nuget.org/packages/Asp.Versioning.OData)

Expands upon the service API versioning for ASP.NET Core and adds OData-specific features for your OData v4.0
applications and OData controllers, including support for versioned Entity Data Models (EDMs). The
[API Explorer][explorer-odata] and [OpenAPI][openapi] extensions provided everything you need to document your services.

### ASP.NET (Classic)

#### [Web API](https://www.nuget.org/packages/Asp.Versioning.WebApi)

Everything you need to add service API versioning to your Web API applications and controller classes. The
[API Explorer][explorer-old] extensions provided everything you need to document your services.

#### [OData](https://www.nuget.org/packages/Asp.Versioning.WebApi.OData)

Expands upon the service API versioning for Web API and adds OData-specific features for your OData v4.0 applications
and OData controllers, including support for versioned Entity Data Models (EDMs). The
[API Explorer][explorer-odata-old] extensions provided everything you need to document your services.

## Contributing

ASP.NET API Versioning is free and open source. You can find the source code on [GitHub] and issues and feature requests
can be posted on the [GitHub issue tracker]. ASP.NET API Versioning relies on the community to fix bugs and add
features: if you'd like to contribute, please read the [CONTRIBUTING] guide and consider opening a [pull request].

## License

This project is licensed under the [MIT] license.

[explorer]: https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer
[explorer-grpc]: https://www.nuget.org/packages/Asp.Versioning.Grpc.ApiExplorer
[explorer-odata]: https://www.nuget.org/packages/Asp.Versioning.OData.ApiExplorer
[openapi]: https://www.nuget.org/packages/Asp.Versioning.OpenApi.ApiExplorer
[explorer-old]: https://www.nuget.org/packages/Asp.Versioning.WebApi.ApiExplorer
[explorer-odata-old]: https://www.nuget.org/packages/Asp.Versioning.WebApi.OData.ApiExplorer
[GitHub]: https://github.com/dotnet/aspnet-api-versioning
[GitHub issue tracker]: https://github.com/dotnet/aspnet-api-versioning/issues
[CONTRIBUTING]: https://github.com/dotnet/aspnet-api-versioning/blob/main/docs/CONTRIBUTING.md
[pull request]: https://github.com/dotnet/aspnet-api-versioning/pulls
[MIT]: https://github.com/dotnet/aspnet-api-versioning/blob/main/LICENSE.txt
[Microsoft REST Guidelines]: https://github.com/Microsoft/api-guidelines
[Microsoft REST Guidelines for versioning]: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning