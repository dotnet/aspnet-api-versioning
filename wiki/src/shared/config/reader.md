# API Version Reader

The `IApiVersionReader` interface defines the behavior of how an API version is read in its raw, unparsed form from the
current HTTP request. There are multiple methods for reading an API version provided out-of-the-box or you can implement
your own. The default, configured API version reader is a composed instance `QueryStringApiVersionReader` and
`UrlSegmentApiVersionReader`.

## Query String

The `QueryStringApiVersionReader` reads the requested API version from the requested query string. The default query
string parameter name is **api-version**.  The constructor for this class accepts the name of a query string parameter
so that an alternate query string parameter can be used.

```c#
// svc?api-version=2.0
AddApiVersioning( options => options.ApiVersionReader = new QueryStringApiVersionReader() );
```

```c#
// svc?v=2.0
AddApiVersioning( options => options.ApiVersionReader = new QueryStringApiVersionReader( "v" ) );
```

## Media Type

The `MediaTypeApiVersionReader` reads the requested API version from a HTTP media type request header. The supported
headers are **Content-Type** and **Accept**. If both headers are present, then **Content-Type** is preferred. If the
**Accept** header specifies qualities, then the API version associated with the highest quality is selected. This
behavior is independent of media type negotiation. The default media type parameter is `"v"`, but you may specify an
alternate name. This method of API versioning does not conform to the [Microsoft REST Guidelines]; however, it is
generally accepted as a fully REST-compliant method of versioning.

```c#
// Content-Type: application/json;v=2.0
AddApiVersioning( options => options.ApiVersionReader = new MediaTypeApiVersionReader() );
```

```c#
// Content-Type: application/json;version=2.0
AddApiVersioning( options => options.ApiVersionReader = new MediaTypeApiVersionReader( "version" ) );
```

The `MediaTypeApiVersionReaderBuilder` is also available with additional features that allow:

- Define multiple media type parameters
- Mutually include specific media types
- Mutually exclude specific media types
- Match media types by template
- Match media types by pattern
- Disambiguate between multiple API versions

```c#
// Accept: application/json;v=2.0
AddApiVersioning(
    options =>
    {
        var builder = new MediaTypeApiVersionReaderBuilder();

        options.ApiVersionReader = builder.Parameter( "v" )
                                          .Include( "application/json" )
                                          .Build();
    } );
```

```c#
// Accept: application/vnd.my.company.v1+json
AddApiVersioning(
    options =>
    {
        var builder = new MediaTypeApiVersionReaderBuilder();

        options.ApiVersionReader = builder.Template( "application/vnd.my.company.v{version}+json" )
                                          .Build();
    } );
```

## Header

The `HeaderApiVersionReader` reads the requested API version from a HTTP request header. There is no default or standard
HTTP header. You must define which HTTP header name or names contain the API version information. This method of API
versioning does not conform to the [Microsoft REST Guidelines].

```c#
AddApiVersioning( options => options.ApiVersionReader = new HeaderApiVersionReader( "api-version" ) );
```

## URL Path Segment

The `UrlSegmentApiVersionReader` reads the requested API version from a URL path segment. Extraction of the value is
dependent upon the `ApiVersionRouteConstraint` which is matched by the `ApiVersioningOptions.RouteConstraintName`
property.

```c#
AddApiVersioning( options => options.ApiVersionReader = new UrlSegmentApiVersionReader() );
```

>[!WARNING]
>This method of API versioning violates the REST _Uniform Interface_ constraint and is the slowest of all versioning
>methods because the requested value cannot always easily be extracted from the URL path segment. If you're creating a
>new API, consider using query string or media type versioning instead.

## Composition

Multiple `IApiVersionReader` implementations can be combined using composition instead of inheritance. For convenience,
you can use `ApiVersionReader.Combine` to compose multiple API version reading styles.

```c#
AddApiVersioning(
    options => options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader(),
        new HeaderApiVersionReader() { HeaderNames = { "x-ms-api-version" } } ) );
```


[Microsoft REST Guidelines]: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#12-versioning