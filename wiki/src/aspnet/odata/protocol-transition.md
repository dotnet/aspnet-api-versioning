# Protocol Transitions

One of the primary reasons to version a service is to facilitate changes in behavior and/or data exchange with the
service. In the scope of OData, this can mean transitioning new versions of a service to use the OData protocol or it
can mean existing OData services that are transitioning away from the OData protocol. The API versioning support for
OData enables both of these scenarios.

Consider the following partial controller implementations:

```c#
[ApiVersion( 1.0 )]
public class OrdersController : ApiController
{
    public IHttpActionResult Get() => Ok();
}

[ApiVersion( 2.0 )]
[ControllerName( "Orders" )]
[ODataRoutePrefix( "Orders" )]
public class Orders2Controller : ODataController
{
    [ODataRoute]
    public IHttpActionResult Get() => Ok();
}

[ApiVersion( 3.0 )]
[ControllerName( "Orders" )]
public class Orders3Controller : ApiController
{
    public IHttpActionResult Get() => Ok();
}
```

This set of controllers produce the following semantics for the **Orders** service:

- Version 1.0 of the service uses basic REST semantics and convention-based routing
- Version 2.0 of the service switches to the OData protocol and convention-based routing
- Version 3.0 of the service switches back to basic REST semantics and convention-based routing

>[!IMPORTANT]
>Due to routing limitations in ASP.NET Web API, all versioned routes for a service must be either convention-based or
>attribute-based. Since OData relies on convention-based routing, all routes for a controller with the same name must
>also be convention-based in order for API versioning to function properly.

The configuration required to support this type of scenario will be:

```c#
config.AddApiVersioning();

var modelBuilder = new VersionedODataModelBuilder( config )
{
    ModelConfigurations = { new OrderModelConfiguration() }
};

config.MapVersionedODataRoutes( "odata", "api", modelBuilder );
config.Routes.MapHttpRoute( "orders", "api/{controller}/{id}", new { id = Optional } );
```

You can see a complete end-to-end implementation of this scenario in the [advanced OData Web API sample].

[advanced OData Web API sample]: https://github.com/dotnet/aspnet-api-versioning/tree/main/examples/AspNet/OData/AdvancedODataWebApiExample