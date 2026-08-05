# Diagnostic Code Analysis for ASP.NET API Versioning

.NET compiler platform analyzers inspect application code for code quality and style issues using ASP.NET API
Versioning.

|                                  | ID                  | Category      | Description                                     |
| -------------------------------- | ------------------- | ------------- | ----------------------------------------------- |
| {{#include ../icons/error.md}}   | [AV0001](av0001.md) | Usage         | Invalid API version                             |
| {{#include ../icons/error.md}}   | [AV0002](av0002.md) | Usage         | Invalid API version range                       |
| {{#include ../icons/error.md}}   | [AV0003](av0003.md) | Usage         | Invalid API version status                      |
| {{#include ../icons/error.md}}   | [AV0004](av0004.md) | Usage         | Invalid API version number                      |
| {{#include ../icons/error.md}}   | [AV0005](av0005.md) | Usage         | Invalid API version year                        |
| {{#include ../icons/error.md}}   | [AV0006](av0006.md) | Usage         | Invalid API version month                       |
| {{#include ../icons/error.md}}   | [AV0007](av0007.md) | Usage         | Invalid API version day                         |
| {{#include ../icons/error.md}}   | [AV0008](av0008.md) | Usage         | Invalid API version date                        |
| {{#include ../icons/error.md}}   | [AV0009](av0009.md) | Usage         | Invalid API version format specifier            |
| {{#include ../icons/warning.md}} | [AV0010](av0010.md) | Usage         | Unexpected API version format                   |
| {{#include ../icons/info.md}}    | [AV0011](av0011.md) | Style         | Remove unnecessary default API version          |
| {{#include ../icons/error.md}}   | [AV0012](av0012.md) | Usage         | Invalid default API version                     |
| {{#include ../icons/warning.md}} | [AV0013](av0013.md) | Usage         | Missing AddMvc                                  |
| {{#include ../icons/warning.md}} | [AV0014](av0014.md) | Usage         | Missing API behavior                            |
| {{#include ../icons/warning.md}} | [AV0015](av0015.md) | Performance   | Use a specific API version reader               |
| {{#include ../icons/warning.md}} | [AV0016](av0016.md) | Usage         | Do not assume default API version               |
| {{#include ../icons/info.md}}    | [AV0017](av0017.md) | Usage         | Remove unnecessary default value                |
| {{#include ../icons/error.md}}   | [AV0018](av0018.md) | Usage         | All endpoints are version-neutral               |
| {{#include ../icons/error.md}}   | [AV0019](av0019.md) | Usage         | Versioned and version-neutral                   |
| {{#include ../icons/info.md}}    | [AV0020](av0020.md) | Style         | Remove unnecessary API explorer                 |
| {{#include ../icons/warning.md}} | [AV0021](av0021.md) | Usage         | Use the versioned API explorer                  |
| {{#include ../icons/warning.md}} | [AV0022](av0022.md) | Usage         | Missing AddOData                                |
| {{#include ../icons/warning.md}} | [AV0023](av0023.md) | Usage         | Route components are ignored                    |
| {{#include ../icons/info.md}}    | [AV0024](av0024.md) | Usage         | Remove unnecessary API explorer option          |
| {{#include ../icons/info.md}}    | [AV0025](av0025.md) | Documentation | Missing OpenAPI document description            |
| {{#include ../icons/info.md}}    | [AV0026](av0026.md) | Usage         | Remove unnecessary group name format            |
| {{#include ../icons/warning.md}} | [AV0027](av0027.md) | Usage         | Use DescribeApiVersions                         |
| {{#include ../icons/warning.md}} | [AV0028](av0028.md) | Usage         | Sunset policy takes effect before deprecation   |
| {{#include ../icons/warning.md}} | [AV0029](av0029.md) | Usage         | Remove unnecessary OpenAPI services             |
| {{#include ../icons/warning.md}} | [AV0030](av0030.md) | Usage         | Missing WithDocumentPerVersion                  |
| {{#include ../icons/warning.md}} | [AV0031](av0031.md) | Usage         | Missing API explorer                            |
