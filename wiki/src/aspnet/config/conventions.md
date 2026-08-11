<!-- description: Declare API versions in ASP.NET Web API with conventions instead of attributes. -->

{{#include ../../shared/config/conventions-pre.md}}

```c#
configuration.AddApiVersioning( options =>
{
    options.Conventions.Controller<MyController>().HasApiVersion( 1.0 );
} );
```

All of the semantics that can be expressed with .NET attributes can be defined using conventions. Consider what version
`2.0` of the previous controller with interleaved API versions might look like:

```c#
[RoutePrefix( "my" )]
public class MyController : ApiController
{
    [Route]
    public IHttpActionResult Get() => Ok();

    [Route]
    public IHttpActionResult GetV2() => Ok();

    [Route( "{id:int}" )]
    public IHttpActionResult GetV2( int id ) => Ok();
}
```

The API version conventions might then be defined as:

```c#
options.Conventions.Controller<MyController>()
                   .HasDeprecatedApiVersion( 1.0 )
                   .HasApiVersion( 2.0 )
                   .Action( c => c.GetV2() ).MapToApiVersion( 2.0 )
                   .Action( c => c.GetV2( default ) ).MapToApiVersion( 2.0 );
```

If you use API version conventions and .NET attributes, then the constructed `ApiVersionModel` for the corresponding
controller will be an aggregated union of the two sets of information.

## Custom

You can also define custom conventions via the `IControllerConvention` interface and add them to the builder:

```c#
public interface IControllerConvention
{
    bool Apply( IControllerConventionBuilder controller,
                HttpControllerDescriptor controllerDescriptor );
}
```

Custom conventions are added to the convention builder through the API versioning options:

```c#
options.Conventions.Add( new MyCustomConvention() );
```

{{#include ../../shared/config/conventions-post.md}}