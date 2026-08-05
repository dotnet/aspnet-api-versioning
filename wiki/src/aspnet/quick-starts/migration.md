{{#include ../../shared/quick-starts/migration-overview.md}}

## Package Identifiers

The original `Microsoft.*` packages are now deprecated and will only undergo servicing:

| Package                                        | Version  | TFM                   |
| ---------------------------------------------- | -------- | --------------------- |
| Microsoft.AspNet.WebApi.Versioning             | <= 5.x.x | net45                 |
| Microsoft.AspNet.WebApi.Versioning.ApiExplorer | <= 5.x.x | net45                 |
| Microsoft.AspNet.OData.Versioning              | <= 5.x.x | net45                 |
| Microsoft.AspNet.OData.Versioning.ApiExplorer  | <= 5.x.x | net45                 |

All new features and platform support will use the `Asp.Versioning.*` prefix:

| Package                                    | Version | TFM                                     |
| ------------------------------------------ | ------- | --------------------------------------- |
| Asp.Versioning.Abstractions                | 6.0.0+  | net6.0+, netstandard1.0, netstandard2.0 |
| Asp.Versioning.WebApi                      | 6.0.0+  | net45, net472                           |
| Asp.Versioning.WebApi.ApiExplorer          | 6.0.0+  | net45, net472                           |
| Asp.Versioning.WebApi.OData                | 6.0.0+  | net45, net472                           |
| Asp.Versioning.WebApi.OData.ApiExplorer    | 6.0.0+  | net45, net472                           |

{{#include ../../shared/quick-starts/migration-common.md}}