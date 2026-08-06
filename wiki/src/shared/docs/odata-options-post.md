### Use Qualified Names

The OData API Explorer is responsible for building URLs that refer to your entity sets, functions, and actions. This
property determines whether the constructed URLs use qualified names. The default value is `false`. The
`ODataUriResolver` instance configured for your application must be configured to match the generated URLs
(ex: `UnqualifiedCallAndEnumPrefixFreeResolver`).

### Query Options

This option allows you to configure OData query options. The configuration for query options can be expressed purely by
convention, through the use of supported OData query attribute, or both. The default behavior will always apply
conventions from OData query attributes without additional configuration. For more information see the
[OData query options](odata-options.md#query-options) topic.

### Metadata Options

This option allows you to determine whether the OData metadata (`$metadata`) and service document (`/`) are explored as
available endpoints. The available options are: `None`, `ServiceDocument`, `Metadata`, or `All`. The default value is
`None`.

### Ad Hoc Model Builder

This property returns an `VersionedODataModelBuilder` that can be used for building ad hoc Entity Data Models (EDMs)
that are used when defining the query options for APIs that do **not** use the full OData stack. Some OData query
options can **only** be set via _Model Bound_ settings. This builder constructs an ad hoc EDM that will contain those
settings solely for the purposes of API exploration and without opting into any other OData-specific features. For more
information see the [OData query options](odata-options.md#query-options) topic.

### Related Entity Id Parameter Description

This option enables you to specify the description for OData related entity links. The default value is
`"The identifier of the related entity."` OData related entity links appear in `$ref` requests. This description is
used to describe dynamic parameters such as the `$id` query parameter.