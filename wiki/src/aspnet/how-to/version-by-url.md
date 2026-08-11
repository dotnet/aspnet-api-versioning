<!-- description: Version an ASP.NET Web API by URL path segment. -->

{{#include ../../shared/how-to/version-by-url-pre.md}}

### Web API

```c#
public static class WebApiConfig
{
    public static void Configuration( HttpConfiguration configuration )
    {
        var constraintResolver = new DefaultInlineConstraintResolver()
        {
            ConstraintMap =
            {
                ["apiVersion"] = typeof( ApiVersionRouteConstraint )
            }
        };
        configuration.MapHttpAttributeRoutes( constraintResolver );
        configuration.AddApiVersioning();
    }
}
```

```c#
[ApiVersion( 1.0 )]
[Route( "api/v{version:apiVersion}/helloworld" )]
public class HelloWorldController : ApiController
{
    public string Get() => "Hello world!";
}

[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[Route( "api/v{version:apiVersion}/helloworld" )]
public class HelloWorld2Controller : ApiController
{
    public string Get() => "Hello world v2!";

    [MapToApiVersion( 3.0 )]
    public string GetV3() => "Hello world v3!";
}
```

### OData

Since the OData implementation uses convention-based routes under the hood, the `ApiVersionRouteConstraint` is
automatically added to all versioned OData routes when needed. The name of the constraint used in prefixes of OData
routes must be `apiVersion` and cannot be changed.

```c#
public static class WebApiConfig
{
    public static void Configuration( HttpConfiguration configuration )
    {
        var modelBuilder = new VersionedODataModelBuilder( configuration )
        {
            ModelConfigurations =
            {
                new PersonModelConfiguration()
            }
        };

        configuration.AddApiVersioning();
        configuration.MapVersionedODataRoutes( "odata-bypath", "api/v{apiVersion}", modelBuilder );
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
    public IQueryable<Person> Get() => new[]{ new Person() }.AsQueryable();
}

[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[ControllerName( "People" )]
[ODataRoutePrefix( "People" )]
public class People2Controller : ODataController
{
    [EnableQuery]
    [ODataRoute]
    public IQueryable<Person> Get() => new[]{ new Person() }.AsQueryable();

    [EnableQuery]
    [ODataRoute, MapToApiVersion( 3.0 )]
    public IQueryable<Person> GetV3() => new[]{ new Person() }.AsQueryable();
}
```

{{#include ../../shared/how-to/version-by-url-post.md}}