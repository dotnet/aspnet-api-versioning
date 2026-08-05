# Versioned Metadata

In order to support API versioning, the default `MetadataController` is replaced with a `VersionedMetadataController`
implementation. The main difference between the two is that the `VersionedMetadataController` will return service
document and entity data model (EDM) information for each defined API version.

```c#
[ReportApiVersions]
public class VersionedMetadataController : MetadataController
{
    // omitted for brevity
}
```

Clients can now build proxies that have an affinity to a specific API version.

- `~/$metadata`
- `~/$metadata?api-version=1.0`
- `~/$metadata?api-version=2.0`
- `~/$metadata?api-version=3.0`

If a client does not specify an API version, the assumed value will be the [configured default API version]. When a
client is ready to adopt a new version of the service, they can update their tooling to point to the appropriate API
version of the metadata endpoint and generate a new proxy based on the version-specific EDMX.

## Tooling Support

The `VersionedMetadataController` also supports the HTTP `OPTIONS` method. This allows tools to query the service
document (`~/`) or `$metadata` endpoints and provide a client with choices as to which API version they would like to
create an OData client for.

For example, a tool can query the metadata endpoint:

```http
OPTIONS /$metadata HTTP/2
host: my.api.com
```

which will produce a response that looks like:

```http
HTTP/2 200
allow: GET, OPTIONS
odata-version: 4.0
api-supported-versions: 1.0, 2.0, 3.0
api-deprecated-versions: 0.9
deprecation: @1640995200
sunset: Thu, 01 Apr 2022 00:00:00 GMT
link: <https://docs.api.com/policies.html>; rel="deprecation"; title="API Policy"; type="text/html"
link: <https://docs.api.com/policies.html>; rel="sunset"; title="API Policy"; type="text/html"
link: </openapi/v1.json>; rel="openapi"; title="OpenAPI"; type="application/json"
```

A tool can choose to use this information is several ways. Any supported or deprecated API version is allowable. User
interface tools should filter out deprecated API versions by default, but it could alternatively provide warning if a
deprecated version is selected or will sunset in the near future. Tools that do not afford user interaction will
likely select the highest supported API version. Tools should also consider that the `api-supported-versions` and
`api-deprecated-versions` HTTP headers can be reported multiple times as defined in [RFC 2616 §4.2].

An OData service which does not support API versioning should return with HTTP `501` (Not Implemented) as defined in
[OData: Protocol §9.3.1] of the OData v4.0 specification. However, given that API versioning behaviors of an OData
service are not explicitly defined in the OData protocol, a client may also respond with HTTP `405` (Method Not
Allowed). Tools should graceful fallback to the standard metadata query operations when API versioning information is
unavailable.

[configured default API version]: ../config/options.md
[RFC 2616 §4.2]: https://tools.ietf.org/html/rfc2616#section-4.2
[OData: Protocol §9.3.1]: http://docs.oasis-open.org/odata/odata/v4.0/os/part1-protocol/odata-v4.0-os-part1-protocol.html#_Toc372793653