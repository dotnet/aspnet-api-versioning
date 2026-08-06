; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 10.2.0

### New Rules

Rule ID | Category      | Severity | Notes
--------|---------------|----------|--------------------------------------------------------
AV0011  | Style         | Info     | Remove unnecessary default API version
AV0012  | Usage         | Error    | Invalid default API version
AV0013  | Usage         | Warning  | Missing AddMvc
AV0014  | Usage         | Warning  | Missing API behavior
AV0015  | Performance   | Warning  | Use a specific API version reader
AV0016  | Usage         | Warning  | Do not assume default API version
AV0017  | Usage         | Info     | Remove unnecessary default value
AV0018  | Usage         | Error    | All endpoints are version-neutral
AV0019  | Usage         | Error    | An API cannot be versioned and version-neutral at the same time
AV0020  | Style         | Info     | Remove unnecessary API explorer
AV0021  | Usage         | Warning  | Use the versioned API explorer
AV0022  | Usage         | Warning  | Missing AddOData
AV0023  | Usage         | Warning  | Route components are ignored
AV0024  | Usage         | Info     | Remove unnecessary API explorer option
AV0025  | Documentation | Info     | Missing OpenAPI document description
AV0026  | Usage         | Info     | Remove unnecessary group name format
AV0027  | Usage         | Warning  | Use DescribeApiVersions
AV0028  | Usage         | Warning  | Sunset policy takes effect before deprecation
AV0029  | Usage         | Warning  | Remove unnecessary OpenAPI services
AV0030  | Usage         | Warning  | Missing WithDocumentPerVersion
AV0031  | Usage         | Warning  | Missing API explorer
