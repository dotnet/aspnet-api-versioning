## Default Model Configuration

The `DefaultModelConfiguration` property defines a callback that can be used to apply a default model configuration.
Specifying a callback is useful if you have a configuration that applies to all models or if you want to have a single,
inline model configuration.

```c#
var modelBuilder = new VersionedODataModelBuilder( configuration )
{
    DefaultModelConfiguration = ( builder, apiVersion, routePrefix )
    {
        // TODO: default configuration for all models
    }
};
```

## On Model Created

The `OnModelCreated` property is a callback that serves the same purpose as
`ODataConventionModelBuilder.OnModelCreated`. This callback can be used to perform any additional setup or configuration
required after each EDM model is created.

## Get EDM Models

The `GetEdmModels` method behavior is similar to the `ODataModelBuilder.GetEdmModel` method. This method performs the
following actions:
 
- Discover and enumerate each service API version
- For each service API version:
  - Create an `ODataModelBuilder` via the **ModelBuilderFactory**
  - Invoke [IModelConfiguration.Apply][model-config] for each item defined in `ModelConfigurations`, including the `DefaultModelConfiguration`, with the current model builder and API version
  - Invoke `ODataModelBuilder.GetEdmModel` to generate the current EDM model
  - Apply the `ApiVersionAnnotation` with the current API version to the generated EDM model
  - Invoke `OnModelCreated` with the current model builder and generated EDM model, if defined

[model-config]: model-config.md