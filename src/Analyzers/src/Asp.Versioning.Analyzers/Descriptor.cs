// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

internal static partial class Descriptor
{
    private static DiagnosticDescriptor Diagnostic(
        string id,
        string title,
        string category,
        DiagnosticSeverity defaultSeverity,
        string messageFormat )
    {
        var helpLink = $"https://github.com/dotnet/aspnet-api-versioning/wiki/analyzer-rules-{id}";

        return new( id, title, messageFormat, category, defaultSeverity, isEnabledByDefault: true, helpLinkUri: helpLink );
    }
}