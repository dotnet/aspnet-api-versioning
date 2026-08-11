<!-- description: Configure API versioning for ASP.NET Web API. -->

# Configuring Your Application

Although different variations of ASP.NET have distinct application initialization methods, careful consideration was
taken to make the API versioning configuration as similar as possible across all applications models.

The configuration for ASP.NET Web API applications typically occurs in the `Register` method of the **WebApiConfig.cs**
file. To enable API versioning support with the default options, use the following configuration:

```c#
public static void Register( HttpConfiguration configuration )
{
    configuration.AddApiVersioning();

    // remaining web api setup omitted for brevity
}
```

If you intend to use the [URL segment versioning] method, then you also need to register the appropriate route
constraint:

```c#
public static void Register( HttpConfiguration configuration )
{
    var constraintResolver = new DefaultInlineConstraintResolver()
    {
        ConstraintMap =
        {
            ["apiVersion"] = typeof( ApiVersionRouteConstraint ),
        },
    };
    configuration.MapHttpAttributeRoutes( constraintResolver );
    configuration.AddApiVersioning();

    // remaining setup omitted for brevity
}
```

Custom route constraints can only be configured through the `MapHttpAttributeRoutes` method. This method is only
expected to be called once in an application. Since API versioning may be added to an existing application, you must
explicitly add the route constraint to ensure the current configuration does not break.

This is also the same basic setup for OData applications, except that you do not need to add any route constraints or
map attribute routes. OData uses its own route constraints and convention-based routing. For more information, see the
topic on [API versioning with OData].

[URL segment versioning]: ../how-to/version-by-url.md
[API versioning with OData]: ../odata/overview.md