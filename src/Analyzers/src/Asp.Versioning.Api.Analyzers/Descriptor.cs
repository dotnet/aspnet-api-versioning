// Copyright (c) .NET Foundation and contributors. All rights reserved.

// the descriptors are fields rather than properties because that is the only shape the rule which reports a
// missing CompilationEnd tag is able to trace back from a report site. the underscore keeps the rule id and its
// name legible where the fields are used, which is what SA1310 objects to
#pragma warning disable SA1310

namespace Asp.Versioning.Analyzers;

using static Asp.Versioning.Analyzers.Category;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using static Microsoft.CodeAnalysis.WellKnownDiagnosticTags;

internal static class Descriptor
{
    private static DiagnosticDescriptor Diagnostic(
        string id,
        string title,
        string category,
        DiagnosticSeverity defaultSeverity,
        string messageFormat,
        params string[] customTags )
    {
        var helpLink = $"https://dotnet.github.io/aspnet-api-versioning/diagnostic/{id.ToLowerInvariant()}.html";

        return new(
            id,
            title,
            messageFormat,
            category,
            defaultSeverity,
            isEnabledByDefault: true,
            helpLinkUri: helpLink,
            customTags: customTags );
    }

    public static readonly DiagnosticDescriptor AV0011_UnnecessaryDefaultApiVersion =
        Diagnostic(
            "AV0011",
            "Remove unnecessary default API version",
            Style,
            Info,
            "The default API version is 1.0",
            Unnecessary );

    public static readonly DiagnosticDescriptor AV0012_NeutralDefaultApiVersion =
        Diagnostic(
            "AV0012",
            "Invalid default API version",
            Usage,
            Error,
            "The default API version cannot be version-neutral" );

    public static readonly DiagnosticDescriptor AV0013_MissingAddMvc =
        Diagnostic(
            "AV0013",
            "Missing AddMvc",
            Usage,
            Warning,
            "Call Services.AddApiVersioning().AddMvc() to version MVC (Core) controller-based APIs",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0014_MissingApiBehavior =
        Diagnostic(
            "AV0014",
            "Missing API behavior",
            Usage,
            Warning,
            "Add [ApiController] to the controller or assembly" );

    public static readonly DiagnosticDescriptor AV0015_UseSpecificApiVersionReader =
        Diagnostic(
            "AV0015",
            "Use a specific API version reader",
            Performance,
            Warning,
            "Configure 'ApiVersioningOptions.ApiVersionReader = new {0}();' to optimize performance",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0016_DoNotAssumeDefaultApiVersion =
        Diagnostic(
            "AV0016",
            "Do not assume default API version",
            Usage,
            Warning,
            "AssumeDefaultVersionWhenUnspecified = true is only necessary for existing APIs that do not have an explicit API version",
            Unnecessary,
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0017_DoNotSetDefaultValue =
        Diagnostic(
            "AV0017",
            "Remove unnecessary default value",
            Usage,
            Info,
            "The default value is unnecessary",
            Unnecessary );

    public static readonly DiagnosticDescriptor AV0018_AllEndpointsAreVersionNeutral =
        Diagnostic(
            "AV0018",
            "All endpoints are version-neutral",
            Usage,
            Error,
            "At least one endpoint should have an explicit API version",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0019_VersionedAndNeutral =
        Diagnostic(
            "AV0019",
            "An API cannot be versioned and version-neutral at the same time",
            Usage,
            Error,
            "Detected a version-neutral API that also has versioned endpoints",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0020_UnnecessaryEndpointsApiExplorer =
        Diagnostic(
            "AV0020",
            "Remove unnecessary API explorer",
            Style,
            Info,
            "AddApiExplorer() already adds the endpoints API explorer",
            Unnecessary,
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0021_UseVersionedApiExplorer =
        Diagnostic(
            "AV0021",
            "Use the versioned API explorer",
            Usage,
            Warning,
            "Call AddApiVersioning().AddApiExplorer() so that API versions are described",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0022_MissingAddOData =
        Diagnostic(
            "AV0022",
            "Missing AddOData",
            Usage,
            Warning,
            "Call Services.AddApiVersioning().AddOData() to version OData APIs",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0023_IgnoredRouteComponents =
        Diagnostic(
            "AV0023",
            "Route components are ignored",
            Usage,
            Warning,
            "Configure 'AddOData( options => options.AddRouteComponents() )' so that route components are applied per API version",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0024_InheritedApiExplorerOption =
        Diagnostic(
            "AV0024",
            "Remove unnecessary API explorer option",
            Usage,
            Info,
            "The API explorer already uses this value from the API versioning options",
            Unnecessary,
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0025_MissingDocumentDescription =
        Diagnostic(
            "AV0025",
            "Missing OpenAPI document description",
            Documentation,
            Info,
            "Set <Description> in the project or add [assembly: AssemblyDescription] to describe the OpenAPI document" );

    public static readonly DiagnosticDescriptor AV0026_UnusedGroupNameFormat =
        Diagnostic(
            "AV0026",
            "Remove unnecessary group name format",
            Usage,
            Info,
            "FormatGroupName is only used by an API that sets a group name",
            Unnecessary,
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0027_UseDescribeApiVersions =
        Diagnostic(
            "AV0027",
            "Use DescribeApiVersions",
            Usage,
            Warning,
            "Call app.DescribeApiVersions() so that minimal APIs mapped after the services are built are described",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0028_SunsetBeforeDeprecation =
        Diagnostic(
            "AV0028",
            "Sunset policy takes effect before deprecation",
            Usage,
            Warning,
            "An API cannot be sunset before it is deprecated",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0029_UnnecessaryOpenApiServices =
        Diagnostic(
            "AV0029",
            "Remove unnecessary OpenAPI services",
            Usage,
            Warning,
            "AddApiVersioning().AddOpenApi() registers the OpenAPI services that describe API versions",
            Unnecessary,
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0030_MissingDocumentPerVersion =
        Diagnostic(
            "AV0030",
            "Missing WithDocumentPerVersion",
            Usage,
            Warning,
            "Call MapOpenApi().WithDocumentPerVersion() so that a document is generated for each API version",
            CompilationEnd );

    public static readonly DiagnosticDescriptor AV0031_MissingApiExplorer =
        Diagnostic(
            "AV0031",
            "Missing API explorer",
            Usage,
            Warning,
            "Call AddApiVersioning().{0}() so that the OpenAPI document describes the APIs",
            CompilationEnd );
}