# Deprecation and error responses

## Deprecate a version

Mark a version deprecated on the endpoints that still implement it:

```csharp
var api = app.NewVersionedApi();

api.MapGet( "/old", () => "old" )
   .HasDeprecatedApiVersion( 0.9 )
   .HasApiVersion( 1.0 );
```

With `ReportApiVersions = true`, `GET /old?api-version=0.9` still succeeds and the response carries
`api-supported-versions: 1.0, 2.0` and `api-deprecated-versions: 0.9`. The supported list covers all
versions in the API, not only the ones on that endpoint.

## Error responses

All of these are `application/problem+json` with `status` 400 by default:

| Request | `type` ends with |
|---|---|
| No version, no default assumed | `#unspecified` |
| A version nothing implements, such as `api-version=3.0` | `#unsupported` |
| A value that is not a version, such as `api-version=banana` | `#invalid` |

Change the status for unsupported versions with `options.UnsupportedApiVersionStatusCode = 404;`;
the response is still a `#unsupported` problem.

## Version-neutral endpoints

`IsApiVersionNeutral()` means no version is required: `GET /ping` and `GET /ping?api-version=1.0`
both succeed. A neutral endpoint still rejects a version nothing in the API supports:
`GET /ping?api-version=9.0` is a 400 `unsupported`.
