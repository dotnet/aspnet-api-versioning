// Copyright (c) .NET Foundation and contributors. All rights reserved.

// the descriptors are fields rather than properties because that is the only shape the rules which check a
// descriptor are able to trace back from a report site. the underscore keeps the rule id and its name legible
// where the fields are used, which is what SA1310 objects to
#pragma warning disable SA1310

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

    public static readonly DiagnosticDescriptor AV0001_InvalidApiVersionSyntax =
        Diagnostic(
            "AV0001",
            "Invalid API version",
            Usage,
            Error,
            "An API version must be a date or a number, optionally with a status" );

    public static readonly DiagnosticDescriptor AV0002_InvalidApiVersionRangeSyntax =
        Diagnostic(
            "AV0002",
            "Invalid API version range",
            Usage,
            Error,
            "A range must include 1-2 valid API versions, optionally with inclusive ('[', ']') or exclusive ('(', ')') bounds" );

    public static readonly DiagnosticDescriptor AV0003_InvalidApiVersionStatus =
        Diagnostic(
            "AV0003",
            "Invalid API version status",
            Usage,
            Error,
            "An API version status may only be a letter followed by letters or numbers with optional periods in between" );

    public static readonly DiagnosticDescriptor AV0004_InvalidApiVersionNumber =
        Diagnostic(
            "AV0004",
            "Invalid API version number",
            Usage,
            Error,
            "An API version number cannot be negative" );

    public static readonly DiagnosticDescriptor AV0005_InvalidApiVersionYear =
        Diagnostic(
            "AV0005",
            "Invalid API version year",
            Usage,
            Error,
            "An API version year must be between 1 and 9999" );

    public static readonly DiagnosticDescriptor AV0006_InvalidApiVersionMonth =
        Diagnostic(
            "AV0006",
            "Invalid API version month",
            Usage,
            Error,
            "An API version month must be between 1 and 12" );

    public static readonly DiagnosticDescriptor AV0007_InvalidApiVersionDay =
        Diagnostic(
            "AV0007",
            "Invalid API version day",
            Usage,
            Error,
            "An API version day must be between 1 and 31" );

    public static readonly DiagnosticDescriptor AV0008_InvalidApiVersionDate =
        Diagnostic(
            "AV0008",
            "Invalid API version date",
            Usage,
            Error,
            "The specified API version is not a valid date" );

    public static readonly DiagnosticDescriptor AV0009_InvalidApiVersionFormat =
        Diagnostic(
            "AV0009",
            "Invalid API version format",
            Usage,
            Error,
            "The API version format string is malformed and will throw when applied: {0}" );

    public static readonly DiagnosticDescriptor AV0010_UnexpectedApiVersionFormat =
        Diagnostic(
            "AV0010",
            "Unexpected API version format",
            Usage,
            Warning,
            "The API version format specifier '{0}' is only meaningful up to {1} time(s); repeating it {2} times does not produce the expected result" );
}