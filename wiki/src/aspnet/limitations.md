# Known Limitations

## URL Path Segment

API versioning does not fundamentally change how routing works in ASP.NET. When you elect to support API versioning via
a URL path segment, the API version is part of the path considered in routing. There is currently no built-in method to
match a route where the API version URL path segment has not be specified.

The recommended method to enable this scenario is to use _Double Route Registration_ by providing multiple routes for
the corresponding controller actions as follows:

```c#
[ApiVersion( 1.0)]
[RoutePrefix( "api" )]
public class ValuesController : ApiController
{
  // ~/api/values
  // ~/api/v1/values
  [Route( "values" )]
  [Route( "v{version:apiVersion}/values" )]
  public IHttpActionResult Get() => Ok();
}

[ApiVersion( 2.0 )]
[RoutePrefix( "api" )]
public class Values2Controller : ApiController
{
  // ~/api/v2/values
  [Route( "v{version:apiVersion}/values" )]
  public IHttpActionResult Get() => Ok();
}
```

### Alternative

Y&ou can also choose to implement a custom `IDirectRouteProvider` as suggested in [Issue #73].

## Routing

The _Direct Route_ routing mechanism (aka attribute routing) was bolted onto the existing routing infrastructure. The
design of the out-of-the-box router does not account for nor support overlapping routes between convention-based and
attribute-based routes. The result of this behavior is that for each given route, all of the versioned controllers must
use convention-based or attribute-based routes. Mixing the two routing strategies for the same route is not guaranteed
to resolve correctly.

While it would be ideal to implement a router that could support both approaches, the level of effort to achieve this
is high. Furthermore, it's significantly easier to rationalize about versioned controllers from a service author's
perspective if all of the versioned routes follow the same routing strategy.

### OData

The OData support in ASP.NET Web API uses convention-based routing under the hood. If you want to support transitioning
to, or from, OData using API versioning, your `ApiController` types that match the same routes must also use
convention-based routing.

[Issue #73]: https://github.com/dotnet/aspnet-api-versioning/issues/73