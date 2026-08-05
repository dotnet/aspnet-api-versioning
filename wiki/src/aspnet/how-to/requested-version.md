{{#include ../../shared/how-to/requested-version-pre.md}}

### Web API

```c#
[ApiVersion( 1.0 )]
[ApiVersion( 2.0 )]
public class MyController : ApiController
{
    public IHttpActionResult Get()
    {
        var apiVersion = Request.RequestedApiVersion;
        return Ok();
    }

    // supported in 3.0+
    public IHttpActionResult Get( int id, ApiVersion apiVersion ) => Ok();
}
```
