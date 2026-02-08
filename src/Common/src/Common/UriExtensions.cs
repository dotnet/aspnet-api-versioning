// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

internal static class UriExtensions
{
    extension( Uri uri )
    {
        internal string SafePath => uri.GetLeftPart( UriPartial.Path );
    }
}