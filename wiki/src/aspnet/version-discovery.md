<!-- description: Advertise supported and deprecated API versions from an ASP.NET Web API service via response headers. -->

{{#include ../shared/version-discovery.md}}

### Web API

```c#
// OPTIONS ~/api/myservice?api-version=[1.0|2.0|3.0]
[HttpOptions]
public IHttpActionResult Options()
{
    var response = new HttpResponseMessage( HttpStatusCode.OK );
    response.Content = new StringContent( string.Empty );
    response.Content.Headers.Add( "Allow", new[] { "GET", "POST", "OPTIONS" } );
    response.Content.Headers.ContentType = null;
    return ResponseMessage( response );
}
```

```http
HTTP/1.1 200 OK
allow: GET, POST, OPTIONS
api-supported-versions: 1.0, 2.0, 3.0
```