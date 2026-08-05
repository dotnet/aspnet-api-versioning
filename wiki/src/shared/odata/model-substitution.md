# Model Substitution

The Entity Data Model (EDM) does not have a one-to-one correlation with the corresponding .NET type. As a result, it's
common and quite plausible that a single .NET type for a model will be used in different EDMs. This is already supported
by defining [model configurations].

The challenge is representing this same model in the OData API Explorer. API Explorer consumers, such as OpenAPI/Swagger
document generators, rely on using Reflection to enumerate the members of a model. These consumers have no intrinsic
understanding of an EDM and do not know that the response type may be a subset of the discovered .NET type. To address
this, the OData API Explorer supports _Model Substitution_.

Model substitution takes effect whenever a .NET type does not exactly match the definition of the corresponding EDM
type. When this occurs, the API Explorer will generate a new .NET type that is a subset of the original type, but
exactly matches the definition of the EDM type. When consumers use Reflection on the substituted type, it will only be
a subset of the original .NET type. A similar scenario occurs for OData actions because the action parameters are
modeled as a dictionary of key/value pairs. A substitution type will be generated which matches the definition of the
OData action parameters.

There is no configuration or additional setup required to enable _Model Substitution_. As the OData API Explorer would
otherwise report incorrect response types, this feature is automatically enabled and cannot be disabled out-of-the-box.

Model substitution supports the following features:

- Entity Types
- Complex Types
- Structured Type Properties
  - Self-Referencing
  - Parent-Child collections
- Attributes (ex: Model Bound Attributes, Data Annotations, etc)
  - Defined on the original .NET type (ex: class or structure)
  - Defined on the original .NET type property
- Action parameters
- `IEnumerable<T>` response types
- `SingleResult<T>` response types
- `ODataValue<T>` response types
- `Delta<T>` parameters

The OData API Explorer generates substitution types using the `IModelTypeBuilder`.

```c#
public interface IModelTypeBuilder
{
    Type NewStructuredType(
        IEdmStructuredType structuredType,
        Type clrType,
        ApiVersion apiVersion,
        IEdmModel edmModel );

    Type NewActionParameters(
        IServiceProvider services,
        IEdmAction action,
        ApiVersion apiVersion,
        string controllerName );
}
```

## Partial OData

The `DefaultModelTypeBuilder` does **not** enable support for ad hoc models using only part of the OData stack. This is
the default behavior because without an EDM and the OData response writers, no filtering of model members is performed.
This mostly likely means that you have a different model per API version, which would negate the usefulness of model
substitution.

If you have a way to filter you models to match what you have configured in an ad hoc EDM, you can re-enable model
substitution by re-registering `IModelTypeBuilder` with `new DefaultModelTypeBuilder(includeAdHocModels: true)`.

[model configurations]: model-config.md