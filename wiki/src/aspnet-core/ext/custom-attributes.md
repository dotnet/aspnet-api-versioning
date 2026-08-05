{{#include ../../shared/ext/custom-attributes-pre.md}}

```
[V1]
[ApiController]
[Route( "api/[controller]" )]
public class HelloWorldController : ControllerBase
{
    [HttpGet]
    public string Get() => "Hello world!";
}
```

{{#include ../../shared/ext/custom-attributes-post.md}}