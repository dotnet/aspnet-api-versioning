<!-- description: The AV analyzer rules that inspect code using ASP.NET API Versioning for correctness and style issues. -->

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

## Reporting

Most rules report as you type, but some report only when the project is built.

A rule that judges a single expression decides as soon as that expression is written. AV0017, for example, sees an
assignment and has everything it needs. A rule that compares one call against another cannot decide until every file
has been read, because the call it is looking for may be in a file that is not open. AV0028 cannot report a sunset
until it has seen every deprecation, and AV0027 reports because a call is missing, which is only known once there is
nothing left to read.

The rules that report only on build are AV0013, AV0015, AV0016, AV0018, AV0019, AV0020, AV0021, AV0022, AV0023,
AV0024, AV0026, AV0027, AV0028, AV0029, AV0030, and AV0031. The rest report live in the editor.

These rules also report live in an editor configured to analyze the whole solution rather than only the documents
that are open:

- **Visual Studio**: Tools → Options → Text Editor → C# → Advanced → *Run background code analysis for* →
  **Entire solution**
- **Rider**: enable *Solution-Wide Analysis*
- **Visual Studio Code**: `"dotnet.backgroundAnalysis.analyzerDiagnosticsScope": "fullSolution"`

## Suppression

A single rule is configured the same way as any other analyzer, by severity in an `.editorconfig` file:

```ini
[*.cs]
dotnet_diagnostic.AV0028.severity = none
```

All of the rules are turned off at once with a property, which removes the analyzers instead of silencing each rule:

```xml
<PropertyGroup>
 <EnableApiVersioningAnalyzers>false</EnableApiVersioningAnalyzers>
</PropertyGroup>
```

Set it in `Directory.Build.props` to apply it to every project in a solution. The rules are enabled unless the
property is `false`.

>[!IMPORTANT]
>`ExcludeAssets="analyzers"` on a package reference does not turn the rules off. The packages that ship the
>analyzers are also reached through the dependencies of other packages, and NuGet combines the assets from every
>path that reaches a package, so an exclusion on one path is undone by another that has none. Use the property
>above instead.
