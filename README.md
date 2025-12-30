[![.NET Foundation](https://img.shields.io/badge/.NET%20Foundation-blueviolet.svg)](https://dotnetfoundation.org/projects/aspnet-api-versioning)
[![MIT License](https://img.shields.io/github/license/dotnet/aspnet-api-versioning?color=%230b0&style=flat-square)](https://github.com/dotnet/aspnet-api-versioning/blob/main/LICENSE.txt)
[![Build Status](https://dev.azure.com/aspnet-api-versioning/build/_apis/build/status/dotnet.aspnet-api-versioning?branchName=main)](https://dev.azure.com/aspnet-api-versioning/build/_build/latest?definitionId=1&branchName=main)

# ASP.NET API Versioning

<img align="right" width="100px" src="logo.svg" />

The _"Asp"_ project, more formally known as ASP.NET API Versioning, gives you a powerful, but easy-to-use method for
adding API versioning semantics to your new and existing REST services built with ASP.NET. The API versioning extensions
define simple metadata attributes and conventions that you use to describe which API versions are implemented by your
services. You don't need to learn any new routing concepts or change the way you implement your services in ASP.NET today.

The default API versioning configuration is compliant with the
[versioning semantics](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning)
outlined by the [Microsoft REST Guidelines](https://github.com/Microsoft/api-guidelines). There are also a number
of customization and extension points available to support transitioning services that may not have supported API
versioning in the past or supported API versioning with semantics that are different from the
[Microsoft REST versioning guidelines](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning).

The supported flavors of ASP.NET are:

* **ASP.NET Core**
  <div>Adds API versioning to your ASP.NET Core <i>Minimal API</i> applications</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.Http.svg)](https://www.nuget.org/packages/Asp.Versioning.Http)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.Http.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.Http)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/New-Services-Quick-Start#aspnet-core)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/main/examples/AspNetCore/WebApi)

* **ASP.NET Core MVC**
  <div>Adds API versioning to your ASP.NET Core MVC (Core) applications</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.Mvc.svg)](https://www.nuget.org/packages/Asp.Versioning.Mvc)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.Mvc.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.Mvc)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/New-Services-Quick-Start#aspnet-core)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/main/examples/AspNetCore/WebApi)

* **ASP.NET Core and OData**
  <div>Adds API versioning to your ASP.NET Core applications using OData v4.0</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.OData.svg)](https://www.nuget.org/packages/Asp.Versioning.OData)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.OData.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.OData)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/New-Services-Quick-Start#aspnet-core-with-odata-v40)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/main/examples/AspNetCore/OData)

* **ASP.NET Web API**
  <div>Adds API versioning to your Web API applications</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.WebApi.svg)](https://www.nuget.org/packages/Asp.Versioning.WebApi)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.WebApi.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.WebApi)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/New-Services-Quick-Start#aspnet-web-api)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/main/examples/AspNet/WebApi)

* **ASP.NET Web API and OData**
  <div>Adds API versioning to your Web API applications using OData v4.0</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.WebApi.OData.svg)](https://www.nuget.org/packages/Asp.Versioning.WebApi.OData)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.WebApi.OData.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.WebApi.OData)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/New-Services-Quick-Start#aspnet-web-api-with-odata-v40)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/main/examples/AspNet/OData)

This is also the home of the ASP.NET API versioning API explorers that you can use to easily document your REST APIs with OpenAPI:

* **ASP.NET Core Versioned API Explorer**
  <div>Adds additional API explorer support to your ASP.NET Core applications</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.Mvc.ApiExplorer.svg)](https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.Mvc.ApiExplorer.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/API-Documentation#aspnet-core)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/main/examples/AspNetCore/WebApi/OpenApiSample)

* **ASP.NET Core with OData API Explorer**
  <div>Adds additional API explorer support to your ASP.NET Core applications using OData v4.0</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.OData.ApiExplorer.svg)](https://www.nuget.org/packages/Asp.Versioning.OData.ApiExplorer)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.OData.ApiExplorer.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.OData.ApiExplorer)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/API-Documentation#aspnet-core-with-odata)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/main/examples/AspNetCore/OData/OpenApiODataSample)

* **ASP.NET Web API Versioned API Explorer**
  <div>Replaces the default API explorer in your Web API applications</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.WebApi.ApiExplorer.svg)](https://www.nuget.org/packages/Asp.Versioning.WebApi.ApiExplorer)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.WebApi.ApiExplorer.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.WebApi.ApiExplorer)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/API-Documentation#aspnet-web-api)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/main/examples/AspNet/WebApi/OpenApiWebApiSample)

* **ASP.NET Web API with OData API Explorer**
  <div>Adds an API explorer to your Web API applications using OData v4.0</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.WebApi.OData.ApiExplorer.svg)](https://www.nuget.org/packages/Asp.Versioning.WebApi.OData.ApiExplorer)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.WebApi.OData.ApiExplorer.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.WebApi.OData.ApiExplorer)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/API-Documentation#aspnet-web-api-with-odata)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/main/examples/AspNet/OData/OpenApiODataWebApiSample)

The client-side libraries make it simple to create API version-aware HTTP clients.

* **HTTP Client API Versioning Extensions**
  <div>Adds API versioning support to HTTP clients</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Asp.Versioning.Http.Client.svg)](https://www.nuget.org/packages/Asp.Versioning.Http.Client)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Asp.Versioning.Http.Client.svg?color=green)](https://www.nuget.org/packages/Asp.Versioning.Http.Client)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/API-Documentation#http-client)

## Documentation

You can find additional examples, documentation, and getting started instructions in the [wiki](../../wiki).

## Discussion

Have a general question, suggestion, or other feedback? Check out how you can [contribute](docs/CONTRIBUTING.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

## License

This project is licensed under the [MIT](LICENSE.TXT) license.

## .NET Foundation

[<img align="right" width="100px" style="margin:-70px 0px 0px 0px" src="https://dotnetfoundation.org/img/logo_v4.svg" />](https://dotnetfoundation.org/projects/aspnet-api-versioning)
This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

----
> If you are an existing user, please makes sure you review the [release notes](../../releases) between all major and minor package releases.

<div style="text-align:center;margin-top:32px;font-size:small">Logo by <a href="https://sacramento-design.com" target="_blank">Sacramento Design Works</a></div>
