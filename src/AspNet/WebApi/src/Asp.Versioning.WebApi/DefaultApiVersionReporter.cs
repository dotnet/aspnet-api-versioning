// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Net.Http.Headers;
using System.Text;

/// <content>
/// Provides additional implementation specific to ASP.NET Web API.
/// </content>
public partial class DefaultApiVersionReporter
{
    private static void AddApiVersionHeader( HttpResponseHeaders headers, string headerName, IReadOnlyList<ApiVersion> versions )
    {
        if ( versions.Count == 0 || headers.Contains( headerName ) )
        {
            return;
        }

        if ( versions.Count == 1 )
        {
            headers.Add( headerName, versions[0].ToString() );
            return;
        }

        var headerValue = new StringBuilder();

        headerValue.Append( versions[0] );

        for ( var i = 1; i < versions.Count; i++ )
        {
            headerValue.Append( ", " ).Append( versions[i] );
        }

        headers.Add( headerName, headerValue.ToString() );
    }
}