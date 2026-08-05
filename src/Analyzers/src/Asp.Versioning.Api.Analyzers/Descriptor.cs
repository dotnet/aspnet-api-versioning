// Copyright (c) .NET Foundation and contributors. All rights reserved.

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
        var helpLink = $"https://github.com/dotnet/aspnet-api-versioning/wiki/analyzer-rules-{id}";

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

    public static DiagnosticDescriptor AV0011_UnnecessaryDefaultApiVersion { get; } =
        Diagnostic(
            "AV0011",
            "Remove unnecessary default API version",
            Style,
            Info,
            "The default API version is 1.0.",
            Unnecessary );

    public static DiagnosticDescriptor AV0012_NeutralDefaultApiVersion { get; } =
        Diagnostic(
            "AV0012",
            "Invalid default API version",
            Usage,
            Error,
            "The default API version cannot be version-neutral." );

    public static DiagnosticDescriptor AV0013_MissingAddMvc { get; } =
        Diagnostic(
            "AV0013",
            "Missing AddMvc",
            Usage,
            Warning,
            "Call Services.AddApiVersioning().AddMvc() to version MVC (Core) controller-based APIs." );

    public static DiagnosticDescriptor AV0014_MissingApiBehavior { get; } =
        Diagnostic(
            "AV0014",
            "Missing API behavior",
            Usage,
            Warning,
            "Add [ApiController] to the controller or assembly." );

    public static DiagnosticDescriptor AV0015_UseSpecificApiVersionReader { get; } =
        Diagnostic(
            "AV0015",
            "Use a specific API version reader",
            Performance,
            Warning,
            "Configure 'ApiVersioningOptions.ApiVersionReader = new {0}();' to optimize performance." );

    public static DiagnosticDescriptor AV0016_DoNotAssumeDefaultApiVersion { get; } =
        Diagnostic(
            "AV0016",
            "Do not assume default API version",
            Usage,
            Warning,
            "AssumeDefaultVersionWhenUnspecified = true is only necessary for existing APIs that do not have an explicit API version.",
            Unnecessary );

    public static DiagnosticDescriptor AV0017_DoNotSetDefaultValue { get; } =
        Diagnostic(
            "AV0017",
            "Remove unnecessary default value",
            Usage,
            Info,
            "The default value is unnecessary.",
            Unnecessary );

    public static DiagnosticDescriptor AV0018_AllEndpointsAreVersionNeutral { get; } =
        Diagnostic(
            "AV0018",
            "All endpoints are version-neutral",
            Usage,
            Error,
            "At least one endpoint should have an explicit API version." );

    public static DiagnosticDescriptor AV0019_VersionedAndNeutral { get; } =
        Diagnostic(
            "AV0019",
            "An API cannot be versioned and version-neutral at the same time",
            Usage,
            Error,
            "Detected a version-neutral API that also has versioned endpoints." );

    public static DiagnosticDescriptor AV0020_UnnecessaryEndpointsApiExplorer { get; } =
        Diagnostic(
            "AV0020",
            "Remove unnecessary API explorer",
            Style,
            Info,
            "AddApiExplorer() already adds the endpoints API explorer.",
            Unnecessary );

    public static DiagnosticDescriptor AV0021_UseVersionedApiExplorer { get; } =
        Diagnostic(
            "AV0021",
            "Use the versioned API explorer",
            Usage,
            Warning,
            "Call AddApiVersioning().AddApiExplorer() so that API versions are described." );

    public static DiagnosticDescriptor AV0022_MissingAddOData { get; } =
        Diagnostic(
            "AV0022",
            "Missing AddOData",
            Usage,
            Warning,
            "Call Services.AddApiVersioning().AddOData() to version OData APIs." );

    public static DiagnosticDescriptor AV0023_IgnoredRouteComponents { get; } =
        Diagnostic(
            "AV0023",
            "Route components are ignored",
            Usage,
            Warning,
            "Configure 'AddOData( options => options.AddRouteComponents() )' so that route components are applied per API version." );

    public static DiagnosticDescriptor AV0024_InheritedApiExplorerOption { get; } =
        Diagnostic(
            "AV0024",
            "Remove unnecessary API explorer option",
            Usage,
            Info,
            "The API explorer already uses this value from the API versioning options.",
            Unnecessary );

    public static DiagnosticDescriptor AV0025_MissingDocumentDescription { get; } =
        Diagnostic(
            "AV0025",
            "Missing OpenAPI document description",
            Documentation,
            Info,
            "Set <Description> in the project or add [assembly: AssemblyDescription] to describe the OpenAPI document." );

    public static DiagnosticDescriptor AV0026_UnusedGroupNameFormat { get; } =
        Diagnostic(
            "AV0026",
            "Remove unnecessary group name format",
            Usage,
            Info,
            "FormatGroupName is only used by an API that sets a group name.",
            Unnecessary );

    public static DiagnosticDescriptor AV0027_UseDescribeApiVersions { get; } =
        Diagnostic(
            "AV0027",
            "Use DescribeApiVersions",
            Usage,
            Warning,
            "Call app.DescribeApiVersions() so that minimal APIs mapped after the services are built are described." );

    public static DiagnosticDescriptor AV0028_SunsetBeforeDeprecation { get; } =
        Diagnostic(
            "AV0028",
            "Sunset policy takes effect before deprecation",
            Usage,
            Warning,
            "An API cannot be sunset before it is deprecated." );

    public static DiagnosticDescriptor AV0029_UnnecessaryOpenApiServices { get; } =
        Diagnostic(
            "AV0029",
            "Remove unnecessary OpenAPI services",
            Usage,
            Warning,
            "AddApiVersioning().AddOpenApi() registers the OpenAPI services that describe API versions.",
            Unnecessary );

    public static DiagnosticDescriptor AV0030_MissingDocumentPerVersion { get; } =
        Diagnostic(
            "AV0030",
            "Missing WithDocumentPerVersion",
            Usage,
            Warning,
            "Call MapOpenApi().WithDocumentPerVersion() so that a document is generated for each API version." );

    public static DiagnosticDescriptor AV0031_MissingApiExplorer { get; } =
        Diagnostic(
            "AV0031",
            "Missing API explorer",
            Usage,
            Warning,
            "Call AddApiVersioning().{0}() so that the OpenAPI document describes the APIs." );
}