# URL Path Versioning

An alternate, but common, method of API versioning is to use a URL path segment. This approach does not allow implicitly
matching the initial, default API version of a service; therefore, all API versions must be explicitly declared. In
addition, the API version value specified for the URL segment must still conform to the [version format]. The `v` prefix
is **not** part of the API version, but may be included in route templates if you so desire.

>[!IMPORTANT]
>It is not possible to have a default API version for a URL path segment. This means that setting
`ApiVersioningOptions.AssumedDefaultVersionWhenUnspecified` is unlikely to have any affect when you use this method of
versioning. For more information and possible solutions to address this scenario, refer to the [known limitations].

[version format]: ../version-format.md
[known limitations]: ../known-limitations.md#url-path-segment-routing-with-a-default-api-version