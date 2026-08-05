{{#include ../../shared/how-to/version-by-header-pre.md}}

### Web API

```c#
namespace Services.V1
{
    [ApiVersion( 1.0 )]
    [RoutePrefix( "api/helloworld" )]
    public class HelloWorldController : ApiController
    {
        [Route]
        public string Get() => "Hello world!";
    }
}

namespace Services.V2
{
    [ApiVersion( 2.0 )]
    [RoutePrefix( "api/helloworld" )]
    public class HelloWorldController : ApiController
    {
        [Route]
        public string Get() => "Hello world!";

        [Route]
        public string Post( string text ) => text;
    }
}
```

{{#include ../../shared/how-to/version-by-header-post.md}}