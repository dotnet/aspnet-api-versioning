{{#include ../../shared/quick-starts/existing-services.md}}

### Web API

```c#
public static class WebApiConfig
{
    public static void Configuration( HttpConfiguration configuration )
    {
        // allow a client to call you without specifying an api version
        // since we haven't configured it otherwise, the assumed api version will be 1.0
        configuration.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true );

        // remaining configuration omitted for brevity
    }
}

[ApiVersion( 1.0 )] // ← this attribute isn't required, but it's easier to understand
[RoutePrefix( "People" )]
public class PeopleController : ApiController
{
    // GET ~/people
    // GET ~/people?api-version=1.0
    [Route]
    public IHttpActionResult Get() => Ok( new[] { new Person() } );
}

[ApiVersion( 2.0 )]
[RoutePrefix( "People" )]
public class People2Controller : ApiController
{
    // GET ~/people?api-version=2.0
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
        // allow a client to call you without specifying an api version
        // since we haven't configured it otherwise, the assumed api version will be 1.0
        configuration.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true );

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

[ApiVersion( 1.0 )] // ← this attribute isn't required, but it's easier to understand
[ODataRoutePrefix( "People" )]
public class PeopleController : ODataController
{
    // GET ~/people
    // GET ~/people?api-version=1.0
    [EnableQuery]
    [ODataRoute]
    public IHttpActionResult Get() => Ok( new[] { new Person() } );
}

[ApiVersion( 2.0 )]
[ControllerName( "People" )]
[ODataRoutePrefix( "People" )]
public class People2Controller : ODataController
{
    // GET ~/people?api-version=2.0
    [EnableQuery]
    [ODataRoute]
    public IHttpActionResult Get() => Ok( new[] { new Person() } );
}
```