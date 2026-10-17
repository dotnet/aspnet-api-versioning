# Version sources and defaults

## Where the version is read from

The default reader accepts the version from the `api-version` **query string** parameter and from a
**URL segment**. Setting `ApiVersionReader` **replaces** that default; it does not add to it.

```csharp
// header only: "?api-version=2.0" is now ignored and the request fails as unspecified
builder.Services.AddApiVersioning( options =>
    options.ApiVersionReader = new HeaderApiVersionReader( "x-api-version" ) );
```

To accept several sources, combine them explicitly:

```csharp
builder.Services.AddApiVersioning( options =>
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader( "v" ),
        new HeaderApiVersionReader( "x-api-version" ) ) );
```

With the combined reader above, `?v=1.0` and `x-api-version: 2.0` both work, and the default
`api-version` parameter no longer does.

## URL segment versioning

Put the `apiVersion` route constraint in the route template:

```csharp
var api = app.NewVersionedApi();
api.MapGet( "/v{version:apiVersion}/seg", () => "seg" ).HasApiVersion( 1.0 );
```

`GET /v1/seg` and `GET /v1.0/seg` succeed. A version no endpoint supports (`/v2/seg`) does **not**
produce the 400 `unsupported` problem: the route no longer matches and the response is **404**. A
request to `/seg?api-version=1.0` is also 404, because the template requires the segment.

## When the client sends no version

By default an unspecified version fails with 400. To serve a default version instead:

```csharp
builder.Services.AddApiVersioning( options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion( 2, 0 );   // optional; the default is 1.0
} );
```

Assuming a default hides a missing version from clients, so only enable it for services that already had
unversioned clients.
