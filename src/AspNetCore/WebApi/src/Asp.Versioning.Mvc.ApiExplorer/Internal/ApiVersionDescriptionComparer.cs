// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer.Internal;

internal sealed class ApiVersionDescriptionComparer : IComparer<ApiVersionDescription>
{
    public int Compare( ApiVersionDescription? x, ApiVersionDescription? y )
    {
        if ( x is null )
        {
            return y is null ? 0 : -1;
        }

        if ( y is null )
        {
            return 1;
        }

        var result = x.ApiVersion.CompareTo( y.ApiVersion );

        if ( result == 0 )
        {
            result = StringComparer.Ordinal.Compare( x.GroupName, y.GroupName );
        }

        return result;
    }
}