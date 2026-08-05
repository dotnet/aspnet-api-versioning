# Controller Naming Conventions

There are a few implicit conventions to be aware of.

## Always Versioned

Once you opt into API versioning, every API controller has an API version. This is true even if the controller does not
have an explicit attribute or configured convention. When otherwise unspecified, the version applied to a controller
derives from [ApiVersioningOptions.DefaultApiVersion].

## Naming

ASP.NET provides a built-in convention for controller names that use the form `<Name>Controller` where `Controller` will
be trimmed off when exactly that text. API Versioning slightly expands this convention. It will honor the convention of
`<Name>[#]Controller`. This allows you to have two controller types in the same namespace for different API versions,
but for the same resource; for example, `ValuesController` and `Values2Controller` will both have the name `Values`.
Naming is important for grouping controllers together.

Unfortunately, this can cause an issue for service API versioning if you want to split the implementation across
different types. If the defining type is in a different .NET namespace, then there is no issue; however, if they are in
the same namespace there would be a name collision. For example:

[ApiVersioningOptions.DefaultApiVersion]: ../config/api-versioning-options.md#default-api-version