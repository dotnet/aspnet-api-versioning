// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#pragma warning disable CA1812

using static System.String;

internal sealed class ApiVersionModelDebugView
{
    private const string Comma = ", ";
    private readonly ApiVersionModel model;

    public ApiVersionModelDebugView( ApiVersionModel model ) => this.model = model;

    public bool VersionNeutral => model.IsApiVersionNeutral;

    public string Declared => Join( Comma, model.DeclaredApiVersions );

    public string Implemented => Join( Comma, model.ImplementedApiVersions );

    public string Supported => Join( Comma, model.SupportedApiVersions );

    public string Deprecated => Join( Comma, model.DeprecatedApiVersions );
}