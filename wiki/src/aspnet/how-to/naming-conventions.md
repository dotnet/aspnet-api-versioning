<!-- description: The implicit controller naming conventions in ASP.NET Web API. -->

{{#include ../../shared/how-to/naming-conventions-pre.md}}

```c#
namespace My.Services.V1
{
    [ApiVersion( 1.0 )]
    [RoutePrefix( "helloworld" )]
    public class HelloWorldController : ApiController
    {
        [Route]
        public string Get() => "Hello world v1.0!";
    }
}

namespace My.Services.V2
{
    [ApiVersion( 2.0 )]
    [RoutePrefix( "helloworld" )]
    public class HelloWorldController : ApiController
    {
        [Route]
        public string Get() => "Hello world v2.0!";
    }
}
```
>_Controllers separated by .NET namespace_

```c#
namespace My.Services.Controllers
{
    [ApiVersion( 1.0 )]
    [RoutePrefix( "helloworld" )]
    public class HelloWorldController : ApiController
    {
        [Route]
        public string Get() => "Hello world v1.0!";
    }

    [ApiVersion( 2.0 )]
    [RoutePrefix( "helloworld" )]
    public class HelloWorld2Controller : ApiController
    {
        [Route]
        public string Get() => "Hello world v2.0!";
    }
}
```
>_Controllers with different names in the same .NET namespace_

{{#include ../../shared/how-to/naming-conventions-post.md}}

### Attribute

If you do not want to rely on a convention, you can explicitly provide a name using the `ControllerNameAttribute`. This
attribute is particularly useful with OData because the name of the controller must also exactly match the name of the
associated entity set.

```c#
[ApiVersion( 2.0 )]
[RoutePrefix( "helloworld" )]
[ControllerName( "HelloWorld" )]
public class HelloWorld2Controller : ControllerBase
{
    [Route]
    public string Get() => "Hello world v2.0!";
}
```