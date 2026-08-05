{{#include ../../shared/how-to/version-interleaving-pre.md}}

### Web API

```c#
[ApiVersion( 1.0 )]
[RoutePrefix( "api/helloworld" )]
public class HelloWorldController : ApiController
{
    [Route]
    public string Get() => "Hello world v1.0!";
}

[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[RoutePrefix( "api/helloworld" )]
public class HelloWorld2Controller : ApiController
{
    [Route]
    public string Get() => "Hello world v2.0!";

    [Route, MapToApiVersion( 3.0 )]
    public string GetV3() => "Hello world v3.0!";
}
```

### OData

```c#
[ApiVersion( 1.0 )]
[ODataRoutePrefix( "People" )]
public class PeopleController : ODataController
{
    [ODataRoute]
    public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );
}

[ApiVersion( 2.0 )]
[ApiVersion( 3.0 )]
[ControllerName( "People" )]
[ODataRoutePrefix( "People" )]
public class People2Controller : ODataController
{
    [ODataRoute]
    public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );

    [ODataRoute, MapToApiVersion( 3.0 )]
    public IHttpActionResult GetV3( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );
}
```

{{#include ../../shared/how-to/version-interleaving-post.md}}