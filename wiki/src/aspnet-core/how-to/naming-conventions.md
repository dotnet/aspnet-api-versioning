{{#include ../../shared/how-to/naming-conventions-pre.md}}

```c#
namespace My.Services.V1
{
    [ApiVersion( 1.0 )]
    [Route( "[controller]" )]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        public string Get() => "Hello world v1.0!";
    }
}

namespace My.Services.V2
{
    [ApiVersion( 2.0 )]
    [Route( "[controller]" )]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        public string Get() => "Hello world v2.0!";
    }
}
```
>_Controllers separated by .NET namespace_

```c#
namespace My.Services.Controllers
{
    [ApiVersion( 1.0 )]
    [Route( "[controller]" )]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        public string Get() => "Hello world v1.0!";
    }

    [ApiVersion( 2.0 )]
    [Route( "helloworld" )]
    public class HelloWorld2Controller : ControllerBase
    {
        [HttpGet]
        public string Get() => "Hello world v2.0!";
    }
}
```
>_Controllers with different names in the same .NET namespace_

{{#include ../../shared/how-to/naming-conventions-post.md}}

### Attribute

If you do not want to rely on a convention, you can explicitly provide a name using the `ControllerNameAttribute`. The
name provided will be used verbatim for the `[controller]` token, the controller name, and for grouping. This attribute
is particularly useful with OData because the name of the controller must also exactly match the name of the associated
entity set.

```c#
[ApiVersion( 2.0 )]
[ControllerName( "HelloWorld" )]
[Route( "[controller]" )]
public class HelloWorld2Controller : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world v2.0!";
}
```

## API Controllers

A controller is just a controller in ASP.NET Core; there is no distinction between a _UI Controller_ and an _API
Controller_. Some applications mix UI controllers and API controllers together. This will result in all controllers
requiring an API version, which is undesirable for UI controllers. The advent of the `ApiControllerAttribute` made it
possible to disambiguate the two types of controllers.

API Versioning 3.0 introduced two new interfaces:

```c#
interface IApiControllerFilter
{
    IList<ControllerModel> Apply( IList<ControllerModel> controllers );
}

interface IApiControllerSpecification
{
    bool IsSatisifedBy( ControllerModel controller );
}
```

The `IApiControllerFilter` filters which controllers should be considered API controllers. The default implementation
typically does not need to be replaced. The `IApiControllerSpecification` defines a specification as to whether a
particular controller is an API controller.

There are two built-in specifications:

- `ApiBehaviorSpecification` - matches controllers decorated by `[ApiController]`
- `ODataControllerSpecification` - matches controllers decorated by `[ODataRouting]`

 An _API controller_ will be considered any controller that matches at least one specification. If a built-in
 specification does not meet your specific needs, you can create your own:

```c#
// considers controllers inheriting from Controller to be a UI controller
public class NonUIControllerSpecification : IApiControllerSpecification
{
    private readonly Type UIControllerType = typeof( Controller ).GetTypeInfo();

    public bool IsSatisfiedBy( ControllerModel controller ) =>
        !UIControllerType.IsAssignableFrom( controller.ControllerType )
}
```

Register your specification in the services configuration:

```c#
services.TryAddEnumerable(
    ServiceDescriptor.Transient<IApiControllerSpecification, NonUIControllerSpecification>() );
```