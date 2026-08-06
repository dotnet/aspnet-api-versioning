This service implementation will now advertise that API version `1.0` and `2.0` are supported through the
`api-supported-versions` HTTP header even though it has no knowledge about where API version `1.0` is. In a similar
fashion, a service can also advertise deprecated API versions. Note that the [ApiVersioningOptions.ReportApiVersions]
must be enabled for the HTTP headers to be returned in responses.

The only drawback to this approach is that each implementation needs to be updated with the supported and deprecated API
versions when new API versions are released. One possible solution to this limitation is to create an
`IApiVersionProvider` attribute that reads the advertised API versions from a configuration source such as a file or
database. If this is still undesirable, then there is still the option of using HTTP header injection by the host server
or another mechanism to send the supported and deprecated API version information.

[ApiVersioningOptions.ReportApiVersions]: ../config/options.md#report-api-versions