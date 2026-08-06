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

[sunset policies]: ../version-policies.md

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

## Error Responses

The `IErrorResponseProvider` service had been the hook to provide custom error responses. Problem Details ([RFC 7807])
had only just been ratified when this project started and they were not part of ASP.NET yet. ASP.NET Core eventually
added first-class support for Problem Details and `IErrorResponseProvider` had an adapter implementation for alignment
in previous versions. Now that Problem Details are the de factor method for error reporting, it no longer makes sense to
retain `IErrorResponseProvider` and it has been removed.

The error responses bodies provided by `IErrorResponseProvider` complied with the
[Microsoft REST Guidelines error response format], which is itself the error response format used by the OData protocol
(see [OData JSON Format §21.1]). If you need to retain that format, the [Error Response backward compatibility] topic
discusses how to enable it.

`ProblemDetails.Type` could logically be used to model the established error `Code`; however, the value is supposed to
be a URI. For backward compatibility, the existing error codes will be emitted as the `Code` extension in Problem
Details. The [Error Responses] topic provides details for each well-known problem that may be returned in responses.

[RFC 7807]: https://datatracker.ietf.org/doc/html/rfc7807
[Microsoft REST Guidelines error response format]: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#710-response-formats
[OData JSON Format §21.1]: https://docs.oasis-open.org/odata/odata-json-format/v4.01/odata-json-format-v4.01.html#_Toc38457793
[Error Response backward compatibility]: ../errors.md#backward-compatibility
[Error Responses]: ../errors.md