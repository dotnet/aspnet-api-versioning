# Query Options

OData query option conventions allow you to specify information for your OData services without having to rely solely
on .NET attributes. There are a number of reasons why you might uses these conventions. The most common reasons are:

- Centralized management and application of all OData query options
- Define OData query options that cannot be expressed with any OData query attributes
- Apply OData query options to services defined by controllers in external .NET assemblies

The parameter names generated are based on the name of the OData query option and the configuration of the
`ODataUriResolver`. OData supports query options without the system `$` prefix. This is enabled or disabled by the
`ODataUriResolver.EnableNoDollarQueryOptions` property.