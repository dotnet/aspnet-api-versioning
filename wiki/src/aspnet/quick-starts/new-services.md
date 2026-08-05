{{#include ../../shared/quick-starts/new-services.md}}

### Web API

```c#
public static class WebApiConfig
{
    public static void Configuration( HttpConfiguration configuration )
    {
        configuration.AddApiVersioning();
        // remaining configuration omitted for brevity
    }
}
```

```c#
[ApiVersion( 1.0 )]
[RoutePrefix( "People" )]
public class PeopleController : ApiController
{
    [Route]
    public IHttpActionResult Get() => Ok( new[] { new Person() } );
}
```

### OData

```c#
public static class WebApiConfig
{
    public static void Configuration( HttpConfiguration configuration )
    {
        configuration.AddApiVersioning();

        var modelBuilder = new VersionedODataModelBuilder( configuration )
        {
            DefaultModelConfiguration = ( builder, apiVersion, routePrefix ) =>
            {
                builder.EntitySet<Person>( "People" );
            }
        };

        configuration.MapVersionedODataRoutes( "odata", null, modelBuilder );
        // remaining configuration omitted for brevity
    }
}
```

```c#
[ApiVersion( 1.0 )]
[ODataRoutePrefix( "People" )]
public class PeopleController : ODataController
{
    [EnableQuery]
    [ODataRoute]
    public IHttpActionResult Get() => Ok( new[] { new Person() } );
}
```