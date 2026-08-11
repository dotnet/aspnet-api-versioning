<!-- description: Known limitations in ASP.NET Core. -->

# Known Limitations

## URL Path Segment

API versioning does not fundamentally change how routing works in ASP.NET. When you elect to support API versioning via
a URL path segment, the API version is part of the path considered in routing. There is currently no built-in method to
match a route where the API version URL path segment has not be specified.

The recommended method to enable this scenario is to use _Double Route Registration_ by providing multiple routes for
the corresponding controller actions as follows:

```c#
[ApiVersion( 1.0 )]
[ApiController]
[Route( "api/[controller]" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class ValuesController : ControllerBase
{
  // ~/api/values
  // ~/api/v1/values
  [HttpGet]
  public IHttpActionResult Get() => Ok();
}

[ApiVersion( 2.0 )]
[ApiController]
[Route( "api/v{version:apiVersion}/values" )]
public class Values2Controller : ControllerBase
{
  // ~/api/v2/values
  [HttpGet]
  public IHttpActionResult Get() => Ok();
}
```

### Alternative

You can use middleware or other customizations to simplify your implementation. The [Gist] provides one such
implementation that allows URL versioning to external clients, but allows simplified URL mapping internally. For
example, `api/v1/values` becomes `api/values` internally, captures the `1.0` API version, and sets the requested API
version via the `IApiVersioningFeature`.

[Gist]: https://gist.github.com/fernando-almeida/2b1f59e5f7f99a2f31d95471b895f625