{{#include ../../shared/odata/model-config.md}}

## Dependency Injection

Dependency injection (DI) is a first-class concept in ASP.NET Core. This intrinsic capability enables API versioning to
automatically register all discovered implementations of **IModelConfiguration**. API versioning also registers a
single, but replaceable mapping for `VersionedODataModelBuilder`. This enables you to declare
`VersionedODataModelBuilder` as a dependent parameter wherever you would like ASP.NET Core to inject the configured
instance. The injected instance will always have all of the discovered `IModelConfiguration` instances, but you can
continue to modify the builder until you are ready to create all of the EDMs via
`VersionedODataModelBuilder.GetEdmModels()`.