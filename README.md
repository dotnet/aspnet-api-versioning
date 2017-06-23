[![AppVeyor Build status](https://ci.appveyor.com/api/projects/status/ho6w0yc612i6jmnp?svg=true)](https://ci.appveyor.com/project/Microsoft/aspnet-api-versioning)

# ASP.NET API Versioning

ASP.NET API versioning gives you a powerful, but easy-to-use method for adding API versioning semantics to your new and existing REST services built with ASP.NET. The API versioning extensions define simple metadata attributes and conventions that you use to describe which API versions are implemented by your services. You don't need to learn any new routing concepts or change the way you implement your services in ASP.NET today.

The default API versioning configuration is compliant with the [versioning semantics](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning) outlined by the [Microsoft REST Guidelines](https://github.com/Microsoft/api-guidelines). There are also a number of customization and extension points available to support transitioning services that may not have supported API versioning in the past or supported API versioning with semantics that are different from the [Microsoft REST versioning guidelines](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning).

The supported flavors of ASP.NET are:

* **ASP.NET Web API** (
  [nuget](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Versioning) |
  [quick start](../../wiki/New-Services-Quick-Start#aspnet-web-api) |
  [samples](../../tree/master/samples/webapi) )
  <br>Adds service API versioning to your Web API applications<br>

* **ASP.NET Web API and OData** (
  [nuget](https://www.nuget.org/packages/Microsoft.AspNet.OData.Versioning) |
  [quick start](/wiki/New-Services-Quick-Start#aspnet-web-api-with-odata-v40) |
  [samples](../../tree/master/samples/webapi) )
  <br>Adds service API versioning to your Web API applications using OData v4.0<br>

* **ASP.NET Core** (
  [nuget](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Versioning) |
  [quick start](../../wiki/New-Services-Quick-Start#aspnet-core) |
  [samples](../../tree/master/samples/aspnetcore) )
  <br>Adds service API versioning to your ASP.NET Core applications

This is also the home of the ASP.NET API versioning API explorers that you can use to easily document your REST APIs with Swagger:

* **ASP.NET Web API Versioned API Explorer** (
  [nuget](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Versioning.ApiExplorer) |
  [quick start](../../wiki/API-Documentation#aspnet-web-api) |
  [samples](../../tree/master/samples/webapi/SwaggerWebApiSample) )
  <br> Replaces the default API explorer in your Web API applications<br>

* **ASP.NET Web API with OData API Explorer** (
  [nuget](https://www.nuget.org/packages/Microsoft.AspNet.OData.Versioning.ApiExplorer) |
  [quick start](../../wiki/API-Documentation#aspnet-web-api-with-odata) |
  [samples](../../tree/master/samples/webapi/SwaggerODataWebApiSample) )
  <br>Adds an API explorer to your Web API applications using OData v4.0<br>

* **ASP.NET Core Versioned API Explorer** (
  [nuget](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer) |
  [quick start](../../wiki/API-Documentation#aspnet-core) |
  [samples](../../tree/master/samples/aspnetcore/SwaggerSample) )
  <br>Adds additional API explorer support to your ASP.NET Core applications

You can additional find samples, documentation, and getting started instructions in the [wiki](../../wiki).

----
> If you are an existing user, please makes sure you review the [release notes](../../releases) between all major and minor package releases.
