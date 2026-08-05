# Version Discovery

Requiring an explicit service version helps ensure existing clients don’t break, but we also need a way to advertise
which service versions are currently supported and which versions are deprecated.

To facilitate this need, services should respond with the `api-supported-versions` and `api-deprecated-versions`, which
are multi-value HTTP headers that indicate the supported and deprecated API versions, respectively. A deprecated version
is still implemented, but is expected to be permanently removed in six months or more.  When a version is no longer
supported, it should stop being advertised. Additional information can be provided via [versioning policies].

Reporting API versions is disabled by default. Service authors can enable this behavior for all services by setting the
[ApiVersioningOptions.ReportApiVersions] to true or scoped to individual services by applying the `[ReportApiVersions]`
attribute or the `ReportApiVersions()` convention.

Service authors might also choose to implement the `OPTIONS` method so that clients and tooling can interrogate which
API versions their service supports.

[versioning policies]: version-policies.md
[ApiVersioningOptions.ReportApiVersions]: config/api-versioning-options.md