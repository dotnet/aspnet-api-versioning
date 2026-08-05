```c#
[ApiVersion( 1.0 )]
[ApiController]
[Route( "[controller]" )]
public class BooksController : ControllerBase
{
    [HttpGet]
    [Produces( "application/json" )]
    [ProducesResponseType( typeof( IEnumerable<Book> ), 200 )]
    public IActionResult Get( ODataQueryOptions<Book> options ) =>
        Ok( options.ApplyTo( books.AsQueryable() ) );
}
```

When OData query capabilities are used this way, query options can be discovered via `EnableQueryAttribute` or via the
API Explorer extensions. Unfortunately, these are both ultimately limited to what can be expressed via
`ODataQuerySettings` and `ODataValidationSettings`, which does not cover the gambit of all possible OData query options;
for example, the allowable `$filter` properties. These other properties can be configured via _Model Bound_ settings,
but without using the full OData stack there is no Entity Data Model (EDM) to retrieve these annotations from.

To address this limitation, OData query options can now also be explored using an ad hoc EDM. This EDM only exists for
the purposes of query option exploration. Using an ad hoc EDM does not opt into other OData feature and only exists
during exploration. Applying _Model Bound_ settings to an ad hoc model is almost identical to the normal method. If you
want to use attributes, just apply them to your model.

```c#
[Filter( "author", "published" )]
public class Book
{
    public string Id { get; set; }
    public string Author { get; set; }
    public string Title { get; set; }
    public int Published { get; set; }
}
```

Every action that appears to be _OData-like_ will automatically be discovered and its model explored. Discovered models
are registered as a complex type by default. If you prefer to use entities or need additional control over the applied
settings, you can use conventions as well.

```c#
AddODataApiExplorer(
    options =>
    {
        options.AdHocModelBuilder.DefaultModelConfiguration = (builder, version, prefix) =>
        {
            builder.ComplexType<Book>().Filter( "author", "published" );
        };
    } 
)
```

The **AdHocModelBuilder** is part of the `ODataApiExplorerOptions` as opposed to `ODataApiVersioningOptions`. If you
have numerous models and would like to break the settings into different configurations, you can still use
`IModelConfiguration`. `IModelConfiguration` instances are automatically discovered and injected the same way as they
are when using the full OData stack.

```c#
public class BookConfiguration : IModelConfiguration
{
    public void Apply( ODataModelBuilder builder, ApiVersion apiVersion, string? routePrefix )
    {
        builder.EntitySet<Book>( "Books" ).EntityType.Filter( "author", "published" );
    }
}
```
_Model configuration for an ad hoc model; the `routePrefix` will always be `null`._

There is no distinction between an `IModelConfiguration` that is used for ad hoc EDM exploration versus normal model
registration. It is unlikely that you would be mixing the full and partial OData stack. If you are mixing use cases,
then you can tell the difference between models from the provided API version. There should be no scenario where a
model is registered two different ways for the same API version.