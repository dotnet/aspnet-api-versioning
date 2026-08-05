# API Version Selector

The `IApiVersionSelector` interface defines the behavior of how an API version is selected for a given request context.
This service is typically only used when a client has not requested an explicit API version and the
`AssumeDefaultVersionWhenUnspecified` option is enabled. The role of the API version selector is to select the
appropriate API version given the current request and a model of available API versions.

>[!NOTE]
>Although the `IApiVersionSelector` can be used for other scenarios, it is currently only utilized when no API version
>is requested by a client and the server allows this behavior. The selector provides the rules that selects the most
>appropriate API version according to the server. There is no built-in capability to ignore an API version explicitly
>requested by a client.

There are four API version selectors provided out-of-the-box or you can implement your own. The default, configured API
version selector is `DefaultApiVersionSelector`.

## Default

The `DefaultApiVersionSelector` always selects the configured `DefaultApiVersion`, regardless of the request or
available API version information.

## Constant

The `ConstantApiVersionSelector` always selects a user-defined API version, regardless of the request or available API
version information.

```c#
AddApiVersioning(
    options => options.ApiVersionSelector =
        new ConstantApiVersionSelector(
            new ApiVersion( new( 2016, 7, 1 ) ) );
```

## Current
The `CurrentImplementationApiVersionSelector` selects the maximum API version available which does not have a version
status. If no match is found, it falls back to the configured `DefaultApiVersion`. An an example, if the versions `1.0`,
`2.0`, and `3.0-alpha` are available, then `2.0` will be selected because it's the highest, implemented or released API
version.

```c#
AddApiVersioning(
    options => options.ApiVersionSelector =
        new CurrentImplementationApiVersionSelector( options ) );
```

## Lowest
The `LowestImplementedApiVersionSelector` selects the minimum API version available which does not have a version
status. If no match is found, it falls back to the configured `DefaultApiVersion`. As an example, if the versions
`0.9-beta`, `1.0`, `2.0`, and `3.0-alpha` are available, then `1.0` will be selected because it's the lowest,
implemented or released API version. Your services must be decorated with one or more API versions for the selector to
work effectively or it will always select the configured `DefaultApiVersion`.

```c#
AddApiVersioning(
    options => options.ApiVersionSelector =
        new LowestImplementedApiVersionSelector( options ) );
```