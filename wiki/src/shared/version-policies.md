# Version Policies

[Version discovery][discovery] supports advertising which API versions are supported and deprecated via the
`api-supported-versions` and `api-deprecated-versions` respectively. A key limitation of this support is that it does
not indicate when an API version will be deprecated, sunset, nor what the stated policy is.

Version policies introduce support for [RFC 9745] (Deprecation) and [RFC 8594] (Sunset). These will allow an API version 
to indicate when it will be deprecated via the `deprecation` header as well as when it will disappear for good via the
`sunset` header. These headers do not necessarily apply to all API versions; they will only apply to the API version
that was requested. The deprecation and sunset policies can include additional information such as a web page or OpenAPI
document. These additional links will conform to [RFC 8288] (Web Linking).

These capabilities are useful, not only for instrumented clients, but also for tooling. As an example, an API might
support an `OPTIONS` request to retrieve this information for tooling:

```http
OPTIONS /weather?api-version=1.0 HTTP/2
host: localhost
```

```http
HTTP/2 200
allow: GET, POST, OPTIONS
api-supported-versions: 1.0, 2.0, 3.0
api-deprecated-versions: 0.9
deprecation: @1640995200
sunset: Thu, 01 Apr 2022 00:00:00 GMT
link: <https://docs.api.com/policies.html?api-version=1.0>; rel="deprecation"; title="API Policy"; type="text/html"
link: <https://docs.api.com/policies.html?api-version=1.0>; rel="sunset"; title="API Policy"; type="text/html"
link: </openapi/v1.json>; rel="openapi"; title="OpenAPI"; type="application/json"
```

This indicates to a client that the requested API version `1.0` was deprecated on January 1, 2022 and will sunset on
April 1, 2022. It also provides links to public documentation that outlines the API versioning policies as well as where
to locate the OpenAPI document.

Policies do not have to have a date. The following scenarios are supported:

- Define a policy by API name and version
- Define a policy by API name for any version
- Define a policy by API version for any API
- A sunset policy may have a date
- A sunset policy can have zero or more links

Supporting a policy with links alone enables advertising a stated policy when you don't know when an API version might
actually be deprecated or sunset, which will be common for the current version of an API. If a policy is defined, it
will be emitted through the existing `IReportApiVersions` service. This service is automatically utilized whenever
`ApiVersioningOptions.ReportApiVersions` is set to `true`, `ReportApiVersionsAttribute` is applied, or the
`ReportApiVersions()` convention is applied.

## Configuration

The configuration is performed the same way across all platforms via:

```c#
AddApiVersioning( options =>
{
  // version 1.0 deprecates 1/1/2022 with a public policy page
  options.Policies.Deprecate( 1.0 )
                  .Effective( 2022, 1, 1 )
                  .Link( "https://docs.api.com/policies/deprecation.html" )
                      .Title( "Version Deprecation Policy" )
                      .Type( "text/html" );

  // version 1.0 sunsets 4/1/2022 with a public policy page
  options.Policies.Sunset( 1.0 )
                  .Effective( 2022, 4, 1 )
                  .Link( "https://docs.api.com/policies/sunset.html" )
                      .Title( "Version Sunset Policy" )
                      .Type( "text/html" );

  // public policy page for version 2.0 without a sunset date
  options.Policies.Sunset( 2.0 )
                  .Link( "https://docs.api.com/policies/sunset.html" )
                      .Title( "Version Sunset Policy" )
                      .Type( "text/html" )
})
```

>[!NOTE]
>It should be noted that although links confirm to [RFC 8288], all configurable links are meant to be specific to API
versioning policies. The provided configuration APIs, therefore, only expose a subset of what is configurable and always
use a relation type of `rel="deprecation"` or `rel="sunset"`. The default implementation can be replaced or extended or
you can use the `LinkHeaderValue` directly in your own code, which exposes the complete feature set.

## API Explorer Integration

The API Explorer extensions will attach the appropriate `DeprecationPolicy` or `SunsetPolicy` to a
`ApiVersionDescription` and `ApiDescription`. The policy for a `ApiVersionDescription` will be for an entire API version,
while the policy for an `ApiDescription` could be for a specific API, version, or combination of both.

The provided information can be used in any number of different ways, but would most likely be used in conjunction with
OpenAPI. There is currently no direct support for a deprecation or sunset policy in OpenAPI, but it can be exposed via
an OpenAPI extension or directly in the API documentation.

[discovery]: version-discovery.md
[RFC 8288]: https://datatracker.ietf.org/doc/html/rfc8288
[RFC 8594]: https://www.rfc-editor.org/rfc/rfc8594.html
[RFC 9745]: https://www.rfc-editor.org/rfc/rfc9745.html