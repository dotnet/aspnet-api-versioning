### Configuration

The configuration will then change the default API version reader as follows:

```c#
.AddApiVersioning( options => options.ApiVersionReader = new HeaderApiVersionReader( "x-ms-version" ) );
```

This will allow clients to request a specific API version by the custom HTTP header `x-ms-version`. For example:

```http
GET api/helloworld HTTP/2
host: localhost
x-ms-version: 1.0
```

```http
HTTP/2 200
host: localhost
content-type: text/plain
content-length: 12

Hello world!
```
