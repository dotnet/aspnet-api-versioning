# ASP.NET API Versioning

| :mega: Check out the [announcement](../../discussions/807) regarding upcoming changes |
|-|

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

* **ASP.NET Web API** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.WebApi) |
  [quick start](../../wiki/New-Services-Quick-Start#aspnet-web-api) |
  [examples](../../tree/main/examples/AspNet/WebApi) )
  <br>Adds API versioning to your Web API applications<br>

* **ASP.NET Web API and OData** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.WebApi.OData) |
  [quick start](../../wiki/New-Services-Quick-Start#aspnet-web-api-with-odata-v40) |
  [examples](../../tree/main/examples/AspNet/OData) )
  <br>Adds API versioning to your Web API applications using OData v4.0<br>

* **ASP.NET Core** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.Http) |
  [quick start](../../wiki/New-Services-Quick-Start#aspnet-core) |
  [examples](../../tree/main/examples/AspNetCore/WebApi) )
  <br>Adds API versioning to your ASP.NET Core _Minimal API_ applications<br>

* **ASP.NET Core MVC** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.Mvc) |
  [quick start](../../wiki/New-Services-Quick-Start#aspnet-core) |
  [examples](../../tree/main/examples/AspNetCore/WebApi) )
  <br>Adds API versioning to your ASP.NET Core MVC (Core) applications<br>
  
* **ASP.NET Core and OData** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.OData) |
  [quick start](../../wiki/New-Services-Quick-Start#aspnet-core-with-odata-v40) |
  [examples](../../tree/main/examples/AspNetCore/OData) )
  <br>Adds API versioning to your ASP.NET Core applications using OData v4.0

This is also the home of the ASP.NET API versioning API explorers that you can use to easily document your REST APIs with OpenAPI:

* **ASP.NET Web API Versioned API Explorer** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.WebApi.ApiExplorer) |
  [quick start](../../wiki/API-Documentation#aspnet-web-api) |
  [examples](../../tree/main/examples/AspNet/WebApi/OpenApiWebApiSample) )
  <br>Replaces the default API explorer in your Web API applications<br>

* **ASP.NET Web API with OData API Explorer** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.WebApi.OData.ApiExplorer) |
  [quick start](../../wiki/API-Documentation#aspnet-web-api-with-odata) |
  [examples](../../tree/main/examples/AspNet/OData/OpenApiODataWebApiSample) )
  <br>Adds an API explorer to your Web API applications using OData v4.0<br>

* **ASP.NET Core Versioned API Explorer** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.Mvc.ApiExplorer) |
  [quick start](../../wiki/API-Documentation#aspnet-core) |
  [examples](../../tree/main/examples/AspNetCore/WebApi/OpenApiSample) )
  <br>Adds additional API explorer support to your ASP.NET Core applications<br> 

* **ASP.NET Core with OData API Explorer** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.OData.ApiExplorer) |
  [quick start](../../wiki/API-Documentation#aspnet-core-with-odata) |
  [examples](../../tree/main/examples/AspNetCore/OData/OpenApiODataSample) )
  <br>Adds additional API explorer support to your ASP.NET Core applications using OData v4.0

The client-side libraries make it simple to create API version-aware HTTP clients.

* **HTTP Client API Versioning Extensions** (
  [nuget](https://www.nuget.org/packages/Asp.Versioning.Http.Client) |
  [quick start](../../wiki/API-Documentation#http-client) )
  <br>Adds API versioning support to HTTP clients<br>

## Documentation

You can find additional examples, documentation, and getting started instructions in the [wiki](../../wiki).

## Discussion

Have a general question, suggestion, or other feedback? Check out how you can [contribute](CONTRIBUTING.md).

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

<footer style="text-align:center;margin-top:32px;">Logo by <a href="https://sacramento-design.com" target="_blank">Sacramento Design Works</a></footer>