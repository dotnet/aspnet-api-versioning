# API Versioning Options

The API Versioning options allows you to configure, customize, and extend the default behaviors when you add API
versioning to your application.

`ApiVersioningOptions` has the following configuration settings:

- [ApiVersionReader](reader.md)
- [ApiVersionSelector][IApiVersionSelector]
- [DefaultApiVersion](#default-api-version)
- [AssumeDefaultVersionWhenUnspecified](#assume-default-version-when-unspecified)
- [ReportApiVersions](#report-api-versions)
- [Policies]
- [Conventions]
- [RouteConstraintName](#route-constraint-name)
- [UnsupportedApiVersionStatusCode](#unsupported-api-version-status-code)

### Assume Default Version When Unspecified

This option enables support for clients to make requests with implicit API versioning. This option is disabled by
default, which means that all clients must send requests with an explicit API version. Services will respond to client
requests that do not specify an API version with either HTTP status code `400` (Bad Request) or HTTP status code `404`
(Not Found), depending whether the requested route exists.

This option should only be enabled when supporting legacy services that did not previously support API versioning.
Forcing existing clients to specify an explicit API version for an existing service introduces a breaking change.
Conceptually, clients in this situation are bound to some API version of a service, but they don't know what it is and
never explicit request it.

When this option is enabled, clients will be able to make a request without specifying a specific API version. The API
version of the service that is selected will be based on the configured [IApiVersionSelector].

### Default API Version

This option defines what the default `ApiVersion` will be for a service without explicit API version information. This
is useful for services that use implicit API versioning in their initial release. This value can also be used for
services that may be defined in external assemblies that are not decorated with any API version information. The
configured, default value is `1.0`.

```c#
AddApiVersioning( options => options.DefaultApiVersion = new ApiVersion( 2.0 ) );
```

### Report API Versions

This option enables sending the `api-supported-versions` and `api-deprecated-versions` HTTP header in responses. When
this option is enabled, it will add the `ReportApiVersionsAttribute` as a global action filter to the application
configuration. If there are any deprecation or sunset [policies](#policies) defined, they will also be included in the
response headers. This option is disabled by default.

```c#
AddApiVersioning( options => options.ReportApiVersions = true );
```

### Conventions

This option allows you to construct API version conventions for each of your services as opposed to using .NET
attributes. You can also choose to additionally use .NET attributes and the union of both sets of defined API version
information will be applied. The default convention builders can be extended and/or replaced in this option. For more
information on using conventions see the [API version conventions][Conventions] topic.

### Route Constraint Name

This option allows you to change the name of the API version route constraint. The default name is `"apiVersion"`.

### Policies

This option allows you to define [API versioning policies][Policies]. This is primarily used to define _deprecation_ and
_sunset_ policies about when an API. Related links, such as to a public policy web page, can also be reported that may
be useful to clients for more information about your API policies.

### Unsupported API Version Status Code

This option allows you to configure the HTTP status code used when an unsupported API version is requested. The default
value is `400` (Bad Request).

While any HTTP status code can be used, the following are the most sensible:

| Status Code | Meaning         | Description |
| ----------- | --------------  | ----------- |
| 400         | Bad Request     | The API doesn't support this version |
| 404         | Not Found       | The API doesn't exist |
| 501         | Not Implemented | The API isn't implemented |

#### Remarks

Regardless of the configured option, when versioning by:

- URL segment, `404` is always returned
- media type, `406` or `415` is always returned

[IApiVersionSelector]: selector.md
[Conventions]: conventions.md
[Policies]: ../version-policies.md