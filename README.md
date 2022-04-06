[![.NET Foundation](https://img.shields.io/badge/.NET%20Foundation-blueviolet.svg)](https://dotnetfoundation.org/projects/aspnet-api-versioning)
[![MIT License](https://img.shields.io/github/license/dotnet/aspnet-api-versioning?color=%230b0&style=flat-square)](https://github.com/dotnet/aspnet-api-versioning/blob/main/LICENSE.txt)
[![Build Status](https://dev.azure.com/aspnet-api-versioning/build/_apis/build/status/dotnet.aspnet-api-versioning.old?branchName=ms)](https://dev.azure.com/aspnet-api-versioning/build/_build/latest?definitionId=3&branchName=ms)

| :mega: Check out the [announcement](../../discussions/807) regarding upcoming changes |
|-|

# ASP.NET API Versioning

ASP.NET API versioning gives you a powerful, but easy-to-use method for adding API versioning semantics to your new and existing REST services built with ASP.NET. The API versioning extensions define simple metadata attributes and conventions that you use to describe which API versions are implemented by your services. You don't need to learn any new routing concepts or change the way you implement your services in ASP.NET today.

The default API versioning configuration is compliant with the [versioning semantics](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning) outlined by the [Microsoft REST Guidelines](https://github.com/Microsoft/api-guidelines). There are also a number of customization and extension points available to support transitioning services that may not have supported API versioning in the past or supported API versioning with semantics that are different from the [Microsoft REST versioning guidelines](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning).

The supported flavors of ASP.NET are:

* **ASP.NET Web API**
  <div>Adds service API versioning to your Web API applications</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Microsoft.AspNet.WebApi.Versioning.svg)](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Versioning)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Microsoft.AspNet.WebApi.Versioning.svg?color=green)](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Versioning)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/New-Services-Quick-Start#aspnet-web-api)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/master/samples/webapi)

* **ASP.NET Web API and OData**
  <div>Adds service API versioning to your Web API applications using OData v4.0</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Microsoft.AspNet.OData.Versioning.svg)](https://www.nuget.org/packages/Microsoft.AspNet.OData.Versioning)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Microsoft.AspNet.OData.Versioning.svg?color=green)](https://www.nuget.org/packages/Microsoft.AspNet.OData.Versioning)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/New-Services-Quick-Start#aspnet-web-api-with-odata-v40)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/master/samples/webapi)

* **ASP.NET Core**
  <div>Adds service API versioning to your ASP.NET Core applications</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Microsoft.AspNetCore.Mvc.Versioning.svg)](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Versioning)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Microsoft.AspNetCore.Mvc.Versioning.svg?color=green)](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Versioning)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/New-Services-Quick-Start#aspnet-core)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/master/samples/aspnetcore)
  
* **ASP.NET Core and OData**
  <div>Adds API versioning to your ASP.NET Core applications using OData v4.0</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Microsoft.AspNetCore.OData.Versioning.svg)](https://www.nuget.org/packages/Microsoft.AspNetCore.OData.Versioning)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Microsoft.AspNetCore.OData.Versioning.svg?color=green)](https://www.nuget.org/packages/Microsoft.AspNetCore.OData.Versioning)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/New-Services-Quick-Start#aspnet-core-with-odata-v40)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/master/samples/aspnetcore)

This is also the home of the ASP.NET API versioning API explorers that you can use to easily document your REST APIs with Swagger:

* **ASP.NET Web API Versioned API Explorer**
  <div>Replaces the default API explorer in your Web API applications</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Microsoft.AspNet.WebApi.Versioning.ApiExplorer.svg)](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Versioning.ApiExplorer)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Microsoft.AspNet.WebApi.Versioning.ApiExplorer.svg?color=green)](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Versioning.ApiExplorer)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/API-Documentation#aspnet-web-api)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/master/samples/webapi/SwaggerWebApiSample)

* **ASP.NET Web API with OData API Explorer**
  <div>Adds an API explorer to your Web API applications using OData v4.0</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Microsoft.AspNet.OData.Versioning.ApiExplorer.svg)](https://www.nuget.org/packages/Microsoft.AspNet.OData.Versioning.ApiExplorer)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Microsoft.AspNet.OData.Versioning.ApiExplorer.svg?color=green)](https://www.nuget.org/packages/Microsoft.AspNet.OData.Versioning.ApiExplorer)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/API-Documentation#aspnet-web-api-with-odata)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/master/samples/webapi/SwaggerODataWebApiSample)

* **ASP.NET Core Versioned API Explorer**
  <div>Adds additional API explorer support to your ASP.NET Core applications</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer.svg)](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer.svg?color=green)](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/API-Documentation#aspnet-core)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/master/samples/aspnetcore/SwaggerSample)

* **ASP.NET Core with OData API Explorer**
  <div>Adds additional API explorer support to your ASP.NET Core applications using OData v4.0</div>

  [![NuGet Package](https://img.shields.io/nuget/v/Microsoft.AspNetCore.OData.Versioning.ApiExplorer.svg)](https://www.nuget.org/packages/Microsoft.AspNetCore.OData.Versioning.ApiExplorer)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/Microsoft.AspNetCore.OData.Versioning.ApiExplorer.svg?color=green)](https://www.nuget.org/packages/Microsoft.AspNetCore.OData.Versioning.ApiExplorer)
  [![Quick Start](https://img.shields.io/badge/quick-start-9B6CD1)](../../wiki/API-Documentation#aspnet-core-with-odata)
  [![Examples](https://img.shields.io/badge/example-code-2B91AF)](../../tree/master/samples/aspnetcore/SwaggerODataSample)

You can find additional samples, documentation, and getting started instructions in the [wiki](../../wiki).

## Discussion

Have a general question, suggestion, or other feedback? Check out how you can [contribute](CONTRIBUTING.md).

## Reporting security issues and bugs

Security issues and bugs should be reported privately, via email, to the Microsoft Security Response Center (MSRC) [secure@microsoft.com](mailto:secure@microsoft.com). You should receive a response within 24 hours. If for some reason you do not, please follow up via email to ensure we received your original message. Further information, including the MSRC PGP key, can be found in the [Security TechCenter](https://technet.microsoft.com/en-us/security/ff852094.aspx).

## Code of conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).  For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

----
> If you are an existing user, please makes sure you review the [release notes](../../releases) between all major and minor package releases.
