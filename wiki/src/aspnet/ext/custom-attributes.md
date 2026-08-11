<!-- description: Implement IApiVersionProvider to create custom API version attributes in ASP.NET Web API. -->

{{#include ../../shared/ext/custom-attributes-pre.md}}

```
[V1]
[RoutePrefix( "api/helloworld" )]
public class HelloWorldController : ApiController
{
    [Route]
    public string Get() => "Hello world!";
}
```

{{#include ../../shared/ext/custom-attributes-post.md}}