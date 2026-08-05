# Migration From Previous Versions

This topic serves as the guide for migrating from version `<= 5.x.x` to version `>= 6.0.0`. The majority of this
information has been outlined in previous [discussions].

>[!TIP]
>If you'd like more information on the background context, you can read the [Hello Project "Asp"] announcement.

For the most part, you can expect the required changes to be a new package identifier and different namespaces. It is
entirely possible that you may update those and find the rest of the code to be identical. The mileage will vary
depending on your level of customization, but you can expect the changes to be trivial in most cases.

[discussions]: https://github.com/dotnet/aspnet-api-versioning/discussions
[Hello Project "Asp"]: https://github.com/dotnet/aspnet-api-versioning/discussions/807

## Package Identifiers

The original `Microsoft.*` packages are now deprecated and will only undergo servicing:

| Platform        | Package                                        | Version  | TFM                   |
| --------------- | ---------------------------------------------- | -------- | --------------------- |
| ASP.NET Web API | Microsoft.AspNet.WebApi.Versioning             | <= 5.x.x | net45                 |
| ASP.NET Web API | Microsoft.AspNet.WebApi.Versioning.ApiExplorer | <= 5.x.x | net45                 |
| ASP.NET Web API | Microsoft.AspNet.OData.Versioning              | <= 5.x.x | net45                 |
| ASP.NET Web API | Microsoft.AspNet.OData.Versioning.ApiExplorer  | <= 5.x.x | net45                 |
| ASP.NET Core    | Microsoft.AspNetCore.Mvc.Versioning            | <= 5.x.x | netcoreapp3.1, net5.0 |
| ASP.NET Core    | Microsoft.AspNetCore.Mvc.ApiExplorer           | <= 5.x.x | netcoreapp3.1, net5.0 |
| ASP.NET Core    | Microsoft.AspNetCore.OData                     | <= 5.x.x | netcoreapp3.1, net5.0 |
| ASP.NET Core    | Microsoft.AspNetCore.OData.ApiExplorer         | <= 5.x.x | netcoreapp3.1, net5.0 |

All new features and platform support will use the `Asp.Versioning.*` prefix:

| Platform        | Package                                    | Version | TFM                                     |
| --------------- | ------------------------------------------ | ------- | --------------------------------------- |
| All             | Asp.Versioning.Abstractions                | 6.0.0+  | net6.0+, netstandard1.0, netstandard2.0 |
| ASP.NET Web API | Asp.Versioning.WebApi                      | 6.0.0+  | net45, net472                           |
| ASP.NET Web API | Asp.Versioning.WebApi.ApiExplorer          | 6.0.0+  | net45, net472                           |
| ASP.NET Web API | Asp.Versioning.WebApi.OData                | 6.0.0+  | net45, net472                           |
| ASP.NET Web API | Asp.Versioning.WebApi.OData.ApiExplorer    | 6.0.0+  | net45, net472                           |
| ASP.NET Core    | Asp.Versioning.Http<sup>1</sup>            | 6.0.0+  | net6.0+                                 |
| ASP.NET Core    | Asp.Versioning.Mvc<sup>2</sup>             | 6.0.0+  | net6.0+                                 |
| ASP.NET Core    | Asp.Versioning.Mvc.ApiExplorer<sup>3</sup> | 6.0.0+  | net6.0+                                 |
| ASP.NET Core    | Asp.Versioning.OData                       | 6.0.0+  | net6.0+                                 |
| ASP.NET Core    | Asp.Versioning.OData.ApiExplorer           | 6.0.0+  | net6.0+                                 |
| All             | Asp.Versioning.Http.Client                 | 6.0.0+  | net6.0+, netstandard1.1, netstandard2.0 |

<sub>[1]</sub> Base library that supports _Minimal APIs_<br/>
<sub>[2]</sub> MVC Core with controller support<br/>
<sub>[3]</sub> Supports exploration of _Minimal APIs_ and controllers

## Namespaces

As the project is no longer part of Microsoft, all namespaces have become `Asp.Versioning.*`. It didn't make sense to
keep using `Microsoft.*` when things don't line up. Furthermore, what namespace should all new code live under?
Continuing to use the `Microsoft` namespace seemed _wrong_. An interesting benefit, however, is that using 
`Api.Versioning.*` allows for more consistency across the ASP.NET Web API and Core implementations. The existing
differences in library namespaces for shared code often led to conditional compiler directives. For ease of use,
extension methods will continue to live in the namespace they correspond to.

## API Version

The format and default implementation has not changed, but parsing has been broken apart. The new `IApiVersionParser`
service has been introduced to support this capability. `ApiVersion.Parse` and `ApiVersion.TryParse` have been removed,
but are replaced by `ApiVersionParser.Default`, which will provide a default implementation.

`ApiVersion.GroupVersion` in .NET 6.0 and beyond is now represented as `DateOnly`. `DateOnly` accurately represents how
a group or date version was always meant to be, but couldn't be represented without introducing its own type due to the
design of `DateTime`. The .NET Standard and .NET Framework representations will continue to use `DateTime`.

## API Version Reader

`IApiVersionReader.Read` now returns `IReadOnlyList<string>` instead of `string?`. There are a few reasons for this
change. First, the _Null Mistake_ is removed as an empty list is completely acceptable. Second, it was entirely possible
for a particular reader implementation to return more than one value. Consider that `?api-version=1.0&api-version=2.0`
would return both `1.0` and `2.0`. In previous versions, the implementation would instead throw
`AmbiguousApiVersionException` that would have to be handled. That behavior becomes problematic for the server to
correctly report the response to the client. Reading multiple API version values in and of itself isn't exceptional,
it's just an invalid client request. `ApiVersionReader.Combine` also enables combining different types of readers
through composition. Readers for different parts of a request are even more likely to return different values.
Refactoring to return a list makes it very simple to return all of the raw API versions provided without any exceptions
and regardless of where they were read from.

## API Version Reporting

`IReportApiVersions.Report` now accepts the entire HTTP response as opposed to just the headers. Accepting only the
headers was an over-normalization that wasn't really necessary. Additional information was also necessary to support
[sunset policies]. The `Report` overload that accepts `Lazy<ApiVersionModel>` has been removed as it's no longer used
or necessary.

[sunset policies]: https://github.com/dotnet/aspnet-api-versioning/wiki/Version-Policies

## API Version Model Extensions

Extension methods related to retrieving an `ApiVersionModel` have been supplanted by the new extension property
`ApiVersionMetadata`.  The previous `GetApiVersionModel()` extension method, for example, was a shortcut for
`GetApiVersionModel(ApiVersionMapping.Explicit)`. A new type -  `ApiVersionMetadata` - has been introduced that unifies
the metadata implementation across ASP.NET platforms.

The following is the mapping between the old and new extension methods or properties:

- `GetApiVersionModel(ApiVersionMapping) → ApiVersionMetadata`
- `GetApiVersionModel() → ApiVersionMetadata.Map(ApiVersionMapping.Explicit)`
- `MappingTo(ApiVersion) → ApiVersionMetadata.MappingTo(ApiVersion)`
- `IsMappedTo(ApiVersion) → ApiVersionMetadata.IsMappedTo(ApiVersion)`




