{{#include ../../shared/how-to/version-by-media-type-pre.md}}

### Minimal API

```c#
var hello = app.NewVersionedApi();
var v1 = hello.MapGroup( "/helloworld" ).HasApiVersion( 1.0 );
var v2 = hello.MapGroup( "/helloworld" ).HasApiVersion( 2.0 );

v1.MapGet( "/", () => "Hello world!" );
v2.MapGet( "/", () => "Hello world!" );
v2.MapPost( "/", (string text) => text );
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

{{#include ../../shared/how-to/version-by-media-type-post.md}}

The specific issues include:

- Mapping
  - `IInputFormatter` to the custom media type
  - `IOutputFormatter` to the custom media type
- OpenAPI
  - Listing all of the consumes media types
  - Listing all of the produces media types