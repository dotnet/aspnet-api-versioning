[![AppVeyor Build status](https://ci.appveyor.com/api/projects/status/ho6w0yc612i6jmnp?svg=true)](https://ci.appveyor.com/project/Microsoft/aspnet-api-versioning)

# ASP.NET API Versioning

ASP.NET API versioning gives you a powerful, but easy-to-use method for adding API versioning semantics to your new and existing REST services built with ASP.NET. The API versioning extensions define simple metadata attributes that you use to describe which API versions are implemented by your services. You don't need to learn any new routing concepts or change the way you implement your services in ASP.NET today.

The default API versioning configuration is compliant with the [versioning semantics](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning) outlined by the [Microsoft REST Guidelines](https://github.com/Microsoft/api-guidelines). There are also a number of customization and extension points available to support transitioning services that may not have supported API versioning in the past or supported API versioning with semantics that are different from the [Microsoft REST versioning guidelines](https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning).

The supported flavors of ASP.NET are:

* [ASP.NET Web API](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Versioning) - Adds service API versioning to your Web API applications
* [ASP.NET Web API and OData](https://www.nuget.org/packages/Microsoft.AspNet.OData.Versioning) - Adds service API versioning to your Web API applications using OData v4.0
* [ASP.NET Core](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Versioning) - Adds service API versioning to your ASP.NET Core applications

You can find samples, documentation, and getting started instructions in the [wiki](https://github.com/Microsoft/aspnet-api-versioning/wiki).
