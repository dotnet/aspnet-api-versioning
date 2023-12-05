// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using System.Buffers;
using System.Globalization;
using System.Text;

/// <content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
public partial class DefaultApiVersionReporter
{
    private static void AddApiVersionHeader( IHeaderDictionary headers, string headerName, IReadOnlyList<ApiVersion> versions )
    {
        if ( versions.Count == 0 || headers.ContainsKey( headerName ) )
        {
            return;
        }

        if ( versions.Count == 1 )
        {
            headers[headerName] = versions[0].ToString();
            return;
        }

        var headerValue = new StringBuilder();
        var provider = CultureInfo.InvariantCulture;
        var pool = ArrayPool<char>.Shared;
        var array = pool.Rent( 34 );
        var buffer = array.AsSpan();

        if ( versions[0].TryFormat( buffer, out var written, default, provider ) )
        {
            headerValue.Append( buffer[..written] );
        }
        else
        {
            headerValue.Append( versions[0] );
        }

        buffer[0] = ',';
        buffer[1] = ' ';

        for ( var i = 1; i < versions.Count; i++ )
        {
            if ( versions[i].TryFormat( buffer[2..], out written, default, provider ) )
            {
                headerValue.Append( buffer[..( written + 2 )] );
            }
            else
            {
                headerValue.Append( ", " ).Append( versions[i] );
            }
        }

        headers[headerName] = headerValue.ToString();
        pool.Return( array );
    }
}