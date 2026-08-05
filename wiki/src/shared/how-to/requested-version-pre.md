# Requested API Version

All of the service API version information is accessible via extension methods and properties. Beginning in version
`3.0`, _Model Binding_ is also supported. These features allow you to determine which API version was requested by a
client as well as determine which versions are supported and deprecated. The API versions provided are automatically
aggregated across all service implementations.

The most common usage is the current, client requested API version: