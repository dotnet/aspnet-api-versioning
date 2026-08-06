// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

using static Asp.Versioning.Analyzers.Category;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

internal static class Descriptor
{
    private static DiagnosticDescriptor Diagnostic(
        string id,
        string title,
        string category,
        DiagnosticSeverity defaultSeverity,
        string messageFormat )
    {
        var helpLink = $"https://dotnet.github.io/aspnet-api-versioning/diagnostic/{id.ToLowerInvariant()}.html";

        return new( id, title, messageFormat, category, defaultSeverity, isEnabledByDefault: true, helpLinkUri: helpLink );
    }

    public static DiagnosticDescriptor AV0001_InvalidApiVersionSyntax { get; } =
        Diagnostic(
            "AV0001",
            "Invalid API version",
            Usage,
            Error,
            "An API version must be a date or a number, optionally with a status." );

    public static DiagnosticDescriptor AV0002_InvalidApiVersionRangeSyntax { get; } =
        Diagnostic(
            "AV0002",
            "Invalid API version range",
            Usage,
            Error,
            "A range must include 1-2 valid API versions, optionally with inclusive ('[', ']') or exclusive ('(', ')') bounds." );

    public static DiagnosticDescriptor AV0003_InvalidApiVersionStatus { get; } =
        Diagnostic(
            "AV0003",
            "Invalid API version status",
            Usage,
            Error,
            "An API version status may only be a letter followed by letters or numbers with optional periods in between." );

    public static DiagnosticDescriptor AV0004_InvalidApiVersionNumber { get; } =
        Diagnostic(
            "AV0004",
            "Invalid API version number",
            Usage,
            Error,
            "An API version number cannot be negative." );

    public static DiagnosticDescriptor AV0005_InvalidApiVersionYear { get; } =
        Diagnostic(
            "AV0005",
            "Invalid API version year",
            Usage,
            Error,
            "An API version year must be between 1 and 9999." );

    public static DiagnosticDescriptor AV0006_InvalidApiVersionMonth { get; } =
        Diagnostic(
            "AV0006",
            "Invalid API version month",
            Usage,
            Error,
            "An API version month must be between 1 and 12." );

    public static DiagnosticDescriptor AV0007_InvalidApiVersionDay { get; } =
        Diagnostic(
            "AV0007",
            "Invalid API version day",
            Usage,
            Error,
            "An API version day must be between 1 and 31." );

    public static DiagnosticDescriptor AV0008_InvalidApiVersionDate { get; } =
        Diagnostic(
            "AV0008",
            "Invalid API version date",
            Usage,
            Error,
            "The specified API version is not a valid date." );

    public static DiagnosticDescriptor AV0009_InvalidApiVersionFormat { get; } =
        Diagnostic(
            "AV0009",
            "Invalid API version format",
            Usage,
            Error,
            "The API version format string is malformed and will throw when applied. {0}" );

    public static DiagnosticDescriptor AV0010_UnexpectedApiVersionFormat { get; } =
        Diagnostic(
            "AV0010",
            "Unexpected API version format",
            Usage,
            Warning,
            "The API version format specifier '{0}' is only meaningful up to {1} time(s); repeating it {2} times does not produce the expected result." );
}