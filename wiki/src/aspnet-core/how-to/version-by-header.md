<!-- description: Version an ASP.NET Core API with an arbitrary HTTP request header. -->

{{#include ../../shared/how-to/version-by-header-pre.md}}

### Minimal API

```c#
var hello = app.NewVersionedApi();

hello.MapGet( "/helloworld", () => "Hello world!" ).HasApiVersion( 1.0 );
```

### MVC (Core)

```c#
namespace Services.V1
{
    [ApiVersion( 1.0 )]
    [ApiController]
    [Route( "api/[controller]" )]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        public string Get() => "Hello world!";
    }
}

namespace Services.V2
{
    [ApiVersion( 2.0 )]
    [ApiController]
    [Route( "api/[controller]" )]
    public class HelloWorldController : ControllerBase
    {
        [HttpGet]
        public string Get() => "Hello world!";

        [HttpPost]
        public string Post( string text ) => text;
    }
}
```

{{#include ../../shared/how-to/version-by-header-post.md}}