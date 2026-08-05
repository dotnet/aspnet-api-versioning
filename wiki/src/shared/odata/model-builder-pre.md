# Versioned Model Builder

The `VersionedODataModelBuilder` is a builder of builders, which enables creating an Entity Data Model (EDM) for each
service API version.

```c#
public class VersionedODataModelBuilder
{
    public Func<ODataModelBuilder> ModelBuilderFactory { get; set; }
    public Action<ODataModelBuilder, ApiVersion, string> DefaultModelConfiguration { get; set; }
    public IList<IModelConfiguration> ModelConfigurations { get; }
    public Action<ODataModelBuilder, IEdmModel> OnModelCreated { get; set; }
    public IEnumerable<IEdmModel> GetEdmModels();
    public virtual IEnumerable<IEdmModel> GetEdmModels(string routePrefix);
}
```

## Model Builder Factory

The `ModelBuilderFactory` property defines a factory function used to initialize a new `ODataModelBuilder` for each
service API version. The default value creates a new instance of the `ODataConventionModelBuilder`.  You can update
this property to substitute your own `ODataModelBuilder` or provide a custom initialization setup.

```c#
var modelBuilder = new VersionedODataModelBuilder( configuration )
{
    ModelBuilderFactory = () => new ODataConventionModelBuilder().EnableLowerCamelCase()
};
```

>[!NOTE]
> Using camel-casing for JSON documents is very common. Beginning 3.0, `EnableLowerCamelCase()` is automatically called.

## Model Configurations

The `ModelConfigurations` property is a collection of [IModelConfiguration][model-config] objects which define the
configuration of one or more models to be applied for each API version. Although it's not required, it's recommended
that you create one [IModelConfiguration][model-config] per entity model.

```c#
var modelBuilder = new VersionedODataModelBuilder( configuration )
{
    ModelConfigurations =
    {
        new PersonModelConfiguration()
    }
};
```