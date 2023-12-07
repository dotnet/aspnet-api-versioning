// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static System.String;

internal sealed class ApiVersionModelDebugView( ApiVersionModel model )
{
    private const string Comma = ", ";

    public bool VersionNeutral => model.IsApiVersionNeutral;

    public string Declared => Join( Comma, model.DeclaredApiVersions );

    public string Implemented => Join( Comma, model.ImplementedApiVersions );

    public string Supported => Join( Comma, model.SupportedApiVersions );

    public string Deprecated => Join( Comma, model.DeprecatedApiVersions );
}