// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
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
            headers.Add( headerName, versions[0].ToString() );
            return;
        }

        var headerValue = new StringBuilder( versions[0].ToString() );

        for ( var i = 1; i < versions.Count; i++ )
        {
            headerValue.Append( ", " ).Append( versions[i] );
        }

        headers.Add( headerName, headerValue.ToString() );
    }
}