// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

internal static class UriExtensions
{
    internal static string SafeFullPath( this Uri uri ) => uri.GetLeftPart( UriPartial.Path );
}