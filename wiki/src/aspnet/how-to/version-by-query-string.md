<!-- description: Version an ASP.NET Web API with a query string parameter. -->

{{#include ../../shared/how-to/version-by-query-string-pre.md}}

### Web API

```c#
[RoutePrefix( "api/helloworld" )]
public class HelloWorldController : ApiController
{
    [Route]
    public string Get() => "Hello world!";
}
```

### OData

```c#
[ODataRoutePrefix( "People" )]
public class PeopleController : ODataController
{
    [ODataRoute]
    public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );
}
```

### Next Version

To create the next version of the controller, you can choose to create a new controller with the same route but
decorate it as API version `2.0`. For example:

#### Web API

```c#
[ApiVersion( 2.0 )]
[RoutePrefix( "api/helloworld" )]
public class HelloWorldController : ApiController
{
    [Route]
    public string Get() => "Hello world!";
}
```

#### OData

```c#
[ApiVersion( 2.0 )]
[ControllerName( "People" )]
[ODataRoutePrefix( "People" )]
public class People2Controller : ODataController
{
    [ODataRoute]
    public IHttpActionResult Get( ODataQueryOptions<Person> options ) =>
        Ok( new[]{ new Person() } );
}
```

{{#include ../../shared/how-to/version-by-query-string-post.md}}