# Versioned Clients


The [Asp.Versioning.Http.Client] package brings client-side extensions that make your `HttpClient` instances
_API version-aware_.

## API Version Writer

The reciprocal to [IApiVersionReader] is `IApiVersionWriter`. As the name implies, the `IApiVersionWriter` is
responsible for writing the configured API version into outgoing requests. The default configured writer is the
`QueryStringApiVersionWriter` using the query parameter name `"api-version"`.

Adding API versions to your `HttpClient` instances can easily be configured using the `IHttpClientFactory` dependency
injection extensions.

```c#
var services = new ServiceCollection();

services.AddHttpClient(
          "MyApi",
          client => client.BaseAddress = new Uri( "https://my.api.com") )
        .AddApiVersion( 1.0 );

var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IHttpClientFactory>();
var client = factory.CreateClient( "MyApi" );

// GET https://my.api.com/data?api-version=1.0
var response = await client.GetAsync( "data" );
```

You can add or replace the default `IApiVersionWriter` with:

```c#
var services = new ServiceCollection();

services.AddSingleton<IApiVersionWriter>( new UrlSegmentApiVersionWriter( "{ver}" ) );
services.AddHttpClient(
          "MyApi",
          client => client.BaseAddress = new Uri( "https://my.api.com/v{ver}") )
        .AddApiVersion( 1 );

var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IHttpClientFactory>();
var client = factory.CreateClient( "MyApi" );

// GET https://my.api.com/v1/data
var response = await client.GetAsync( "data" );
```

The following implementations are provided out-of-the-box:

- `QueryStringApiVersionWriter`
- `HeaderApiVersionWriter`
- `MediaTypeApiVersionWriter`
- `UrlSegmentApiVersionWriter`

Specifying multiple API versions is typically unnecessary; however, if this is a capability you need or want, multiple
writers can be composed together:

```c#
var writer = ApiVersionWriter.Combine(
  new QueryApiVersionWriter( "api-version" ),
  new HeaderApiVersionWriter( "x-ms-api-version" ) );
```

Your application might have multiple clients that communicate to services which use different API versioning methods.
To accommodate these differences, you can specify a specific writer per client.

```c#
var services = new ServiceCollection();

services.AddHttpClient(
          "SomeApi",
          client => client.BaseAddress = new Uri( "https://some.api.com/") )
        .AddApiVersion( 1.0, new QueryApiVersionWriter() );

services.AddHttpClient(
          "OtherApi",
          client => client.BaseAddress = new Uri( "https://other.api.com/v{ver}/") )
        .AddApiVersion( 2, new UrlSegmentApiVersionWriter( "{ver}" ) );
```

If you're not using dependency injection or the `IHttpClientFactory`, you can still configure writers by explicitly
configuring the `ApiVersionHandler`:

```c#
using var client = new HttpClient(
    new ApiVersionHandler(
        new QueryApiVersionWriter(), 
        new ApiVersion( 1, 0 ) )
    {
        InnerHandler = new HttpClientHandler(),
    } );
```

## Notifications

API clients always have a few common questions:

- _"How do I know when an API version is deprecated?"_
- _"How do I know when an API version will be sunset?"_
- _"How do I know when a new API version is available?"_

These questions can now be answered via:

```c#
public interface IApiNotification
{
    Task OnApiDeprecatedAsync( ApiNotificationContext context, CancellationToken cancellationToken );
    Task OnNewApiAvailableAsync( ApiNotificationContext context, CancellationToken cancellationToken );
}
```

Where the notification information provided is:

```c#
public class ApiNotificationContext
{
    public HttpResponseMessage Response { get; }
    public ApiVersion ApiVersion { get; }
    public SunsetPolicy SunsetPolicy { get; }
}
```

If the API reports its versions, then the `ApiVersionHandler` will detect when these events occur and invoke the
appropriate notification. The `ApiVersionHandler` will look for the `api-supported-versions` and
`api-deprecated-versions` HTTP headers by default, but alternate headers may be configured. If a deprecation or sunset
policy is specified by the API, then the deprecation date will be read from the `deprecation` HTTP header and the sunset
date will be read from the `sunset` HTTP header. Any `link` HTTP headers where the relation type is
`rel="deprecation"` or `rel="sunset"` will also be read.

No notifications or actions occur by default. The most logical action to perform when a notification occurs is to log
it. The `ApiVersionHandlerLogger<T>` implements an `IApiNotification` that is paired with an `ILogger<T>` that will:

1. Log a warning message when an API reports that the version requested is deprecated.
2. Log an informational message when an API reports that a newer version than the one requested is available.

Logged messages can be connected to alerts to notify developers when these events occur in an automated fashion.

If you configuration uses dependency injection and `ILogger<ApiVersionHandler>` is a resolvable service,
`ApiVersionHandlerLogger<ApiVersionHandler>` will be used as the default `IApiNotification` implementation unless
configured otherwise.

## API Information

Using API information provided in responses is useful, but not always provided for every request. Furthermore, if
you're onboarding to an API, how do you know which API versions are available or deprecated? How do you know the
policies around these APIs? Detailed information might be provided by OpenAPI, but how do you know where the OpenAPI
documents are?

The most logical way for an API to expose this information is to provide an `OPTIONS` method, which may be
version-specific or version-neutral, that returns all of the available API information. This information is useful for
automation and client tooling.

The `GetApiInformationAsync` extension method for the `HttpClient` provides a prescribed implementation to make the
appropriate `OPTIONS` request and parse its response into:

```c#
using var client = new HttpClient()
{
    BaseAddress = new Uri( "https://my.api.com" ),
};
var info = await client.GeApiInformationAsync( "/?api-version=1.0" );
```
_Request API information_

```http
OPTIONS /?api-version=1.0 HTTP/2
host: my.api.com
```
_HTTP request sent_

```http
HTTP/2 200
api-supported-versions: 2.0
api-deprecated-versions: 1.0
deprecation: @1688169600
sunset: Mon, 01 Jan 2024 00:00:00 GMT
link: <https://api.docs.com/policies/deprecation.html>; rel="deprecation"; type="text/html"
link: <https://api.docs.com/policies/sunset.html>; rel="sunset"; type="text/html"
link: <openapi/v1.json>; rel="openapi"; type="application/json"; api-version="1.0"
```
_HTTP response received_

```c#
public class ApiInformation
{
    public IReadOnlyList<ApiVersion> SupportedApiVersions { get; }
    public IReadOnlyList<ApiVersion> DeprecatedApiVersions { get; }
    public SunsetPolicy SunsetPolicy { get; }
    public IReadOnlyDictionary<ApiVersion, Uri> OpenApiDocumentUrls { get; }
}
```
_Parsed API information_

[Asp.Versioning.Http.Client]: https://www.nuget.org/packages/Asp.Versioning.Http.Client
[IApiVersionReader]: ../../config/reader.md