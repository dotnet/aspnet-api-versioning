### Group Name Format

The group name format is the format string that is applied to the current API version being explored. This resultant,
formatted string is used as the group name for the explored API. The group name is often used in tools such as OpenAPI
to logically group APIs together. For more information and examples on the format specifiers for an API version, see
the [custom API version format strings][version-format] topic.

### Substitute in URL

This option will instruct the API explorer to substitute API version parameters that are in the route template with the
corresponding API version value. When an API version parameter value is substituted, that parameter is also removed
from the parameters associated with the API description. This option is useful for service authors that version by URL
segment and want the API version value automatically populated. For example, the route template
`api/v{version}/resource` for API version 1.0 will become `api/v1/resource` and the API version parameter will be
removed. The default value is `false`.

### Substitution Format

This option is meant to be paired with the **SubstituteApiVersionInUrl** option. This affords service authors control
over how the API version value is formatted before being substituted into route templates. The default value is `VVV`,
but it can be any value according to the available [formatting options][version-format].

### Default API Version

This option defines what the default `ApiVersion` will be for a service without explicit API version information. The
default value is derived from [ApiVersioningOptions.DefaultApiVersion] and should not be changed.

### Default API Version Parameter Description

This option defines what the default description for API version parameters will be. The default value is:
`"The requested API version"`.

### Assume Default Version When Unspecified

This option enables support for clients to make requests with implicit API versioning. This option is used during API
exploration to determine whether the API version parameter is required. The default value is derived from
[ApiVersioningOptions.AssumeDefaultVersionWhenUnspecified] and should not be changed.

### Parameter Source

This option configures how the API exploration process discovers API version parameters. The default value derives from
[ApiVersioningOptions.ApiVersionReader] and should not be changed.

### Add Parameter When Version-Neutral

This options let's you define whether an API version parameter is generated for version-neutral APIs. A version-neutral
API does not require an API version; however, you may not want a client to know the API is version-neutral. By setting
`AddApiVersionParametersWhenVersionNeutral = true`, an API version parameter will be explored, even though it is not
required. The default value is `false`.

### Route Constraint Name

This option defines the name of the route constraint used in route templates. The default value derives from
[ApiVersioningOptions.RouteConstraintName] and should not be changed.

[version-format]: ../version-format.md#custom
[ApiVersioningOptions.DefaultApiVersion]: ../config/options.md#default-api-version
[ApiVersioningOptions.AssumeDefaultVersionWhenUnspecified]: ../config/options.md#assume-default-version-when-unspecified
[ApiVersioningOptions.ApiVersionReader]: ../config/options.md
[ApiVersioningOptions.RouteConstraintName]: ../config/options.md#route-constraint-name