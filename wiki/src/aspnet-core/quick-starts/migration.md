{{#include ../../shared/quick-starts/migration-overview.md}}

## Package Identifiers

The original `Microsoft.*` packages are now deprecated and will only undergo servicing:

| Package                                        | Version  | TFM                   |
| ---------------------------------------------- | -------- | --------------------- |
| Microsoft.AspNetCore.Mvc.Versioning            | <= 5.x.x | netcoreapp3.1, net5.0 |
| Microsoft.AspNetCore.Mvc.ApiExplorer           | <= 5.x.x | netcoreapp3.1, net5.0 |
| Microsoft.AspNetCore.OData                     | <= 5.x.x | netcoreapp3.1, net5.0 |
| Microsoft.AspNetCore.OData.ApiExplorer         | <= 5.x.x | netcoreapp3.1, net5.0 |

All new features and platform support will use the `Asp.Versioning.*` prefix:

| Package                                    | Version | TFM                                     |
| ------------------------------------------ | ------- | --------------------------------------- |
| Asp.Versioning.Abstractions                | 6.0.0+  | net6.0+, netstandard1.0, netstandard2.0 |
| Asp.Versioning.Http<sup>1</sup>            | 6.0.0+  | net6.0+                                 |
| Asp.Versioning.Mvc<sup>2</sup>             | 6.0.0+  | net6.0+                                 |
| Asp.Versioning.Mvc.ApiExplorer<sup>3</sup> | 6.0.0+  | net6.0+                                 |
| Asp.Versioning.OData                       | 6.0.0+  | net6.0+                                 |
| Asp.Versioning.OData.ApiExplorer           | 6.0.0+  | net6.0+                                 |

<sub>[1]</sub> Base library that supports Minimal APIs<br/>
<sub>[2]</sub> MVC Core with controller support<br/>
<sub>[3]</sub> Supports exploration of Minimal APIs and controllers

{{#include ../../shared/quick-starts/migration-common.md}}

## API Behaviors

In versions `>= 2.1.0 && < 6.0.0`, the `ApiVersioningOptions` provided the property `UseApiBehavior`. This setting was a
bridge to the API Behaviors feature introduced in ASP.NET Core 2.1. In earlier versions of ASP.NET Core, there was not a
clear way to disambiguate between a UI and API controller. Adding API Behaviors via `[ApiController]` to a controller or
assembly provided a way to solve that problem. API Versioning subsequently added two new services that align to it:

- `IApiControllerFilter` - filters out non-API controllers
- `IApiControllerSpecification` - determines whether a controller is for an API

The default filter is an aggregation over all specifications. The default specifications look for API Behaviors and
OData routing.

In the `2.1.x` time frame, this was a behavioral breaking change. To facilitate a smoother transition, the
`UseApiBehavior` option was introduced with a value of `false`, which maintained the existing behavior. Starting in
`3.0`, the value defaulted to `true`, which only considers controllers with API Behaviors applied. Starting in `6.0`,
the property has been completely removed as it is no longer necessary.

`IApiControllerFilter` and any of the `IApiControllerSpecification` services can be modified through dependency
injection. To align with the legacy behavior of `UseApiBehavior = false`, you can use the `NoControllerFilter`
implementation:

```c#
builder.Services.AddTransient<IApiControllerFilter, NoControllerFilter>();
builder.Services.AddApiVersioning().AddMvc();
```

## Routing Behaviors

The legacy, convention-based routing with `IActionSelector` has been dropped. Limitations in the original ASP.NET Core
routing design caused a number of issues and inconsistencies, which were resolved when Endpoint Routing was introduced;
especially `405` or `415` responses. The primary reason it continued to be supported was waiting for OData to support
Endpoint Routing, which it does as of `8.0`.

The routing logic has been updated to properly return a response for `404`, `405`, `406`, and `415`. Due to necessary
API Versioning fixes and the way routing works in ASP.NET Core, it is no longer possible to always report `400` when an
API version _could_ be matched, but doesn't. In some of these cases it is also not possible to add `ProblemDetails`;
especially prior to .NET 7 because ASP.NET Core did not provide a hook for it.

What happens when an API version _could_ match, but doesn't has always been a bit of a gray area. The general consensus
seems to be that developers don't care because it's a client error or they expect it to be `404`. These default rule
will continue to return `400` when versioning by query string or header, but that can now be changed via
[ApiVersioningOptions.UnsupportedApiVersionStatusCode]. Versioning by URL segment will always return `404`. Versioning
by media type will always return `406` or `415`.

[ApiVersioningOptions.UnsupportedApiVersionStatusCode]: https://github.com/dotnet/aspnet-api-versioning/wiki/API-Versioning-Options#unsupported-api-version-status-code

The `UseApiVersioning()` middleware in ASP.NET Core has been removed. It never did anything except setup the
`IApiVersioningFeature` in the current request, which doesn't require middleware.

## Configuration

Support for Minimal APIs and OData in ASP.NET Core required some changes to how services are configured in an
application. The new `IApiVersioningBuilder` interface provides a way to hang all API Versioning related extensions off
of. This approach also helps address extension method naming conflicts and scenarios where you might forget to register
another set of required services. If you referenced and enabled everything supported by API Versioning, then your
configuration _might_ look like:

```c#
var builder = WebApplication.CreateBuilder( args );
var services = builder.Services;

services.AddApiVersioning()     // Core services with support for Minimal APIs
        .AddMvc()               // MVC Core with controllers (not full MVC)
        .AddApiExplorer()       // API version-aware API Explorer extensions
        .AddOData()             // API versioning extensions for OData
        .AddODataApiExplorer(); // API version-aware API Explorer extensions for OData
```

### Changes

- As noted above, `ApiVersioningOptions.UseApiBehaviors` has been removed
- `ApiVersioningOptions.Conventions` has been moved to `MvcApiVersioningOptions.Conventions` as API Versioning no longer requires MVC Core
  - To configure conventions, use `.AddMvc(options => options.Conventions = ?)` via the `IApiVersioningBuilder` extension method
- `ApiVersioningOptions.ControllerNameConvention` has been removed as an explicit option, but can be changed via dependency injection
  - To configure a different naming convention, use `builder.Services.AddSingleton<IControllerNameConvention, OriginalControllerNameConvention>()`

[RFC 7807]: https://datatracker.ietf.org/doc/html/rfc7807
[Microsoft REST Guidelines error response format]: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#710-response-formats
[OData JSON Format §21.1]: https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#_Toc38457793
[Error Response backward compatibility]: https://github.com/dotnet/aspnet-api-versioning/wiki/Error-Responses#Backward-Compatibility
[Error Responses]: https://github.com/dotnet/aspnet-api-versioning/wiki/Error-Responses