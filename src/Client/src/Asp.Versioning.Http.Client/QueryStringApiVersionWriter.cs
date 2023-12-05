// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using static System.StringComparison;

/// <summary>
/// Represents an API version writer that writes the value to the query string in a URL.
/// </summary>
public sealed class QueryStringApiVersionWriter : IApiVersionWriter
{
    private readonly string parameterName;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryStringApiVersionWriter"/> class.
    /// </summary>
    /// <remarks>This constructor always uses the "api-version" query string parameter.</remarks>
    public QueryStringApiVersionWriter() => parameterName = "api-version";

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryStringApiVersionWriter"/> class.
    /// </summary>
    /// <param name="parameterName">The query string parameter name to write the API version to.</param>
    public QueryStringApiVersionWriter( string parameterName )
    {
        ArgumentException.ThrowIfNullOrEmpty( parameterName );
        this.parameterName = parameterName;
    }

    /// <inheritdoc />
    public void Write( HttpRequestMessage request, ApiVersion apiVersion )
    {
        ArgumentNullException.ThrowIfNull( request );
        ArgumentNullException.ThrowIfNull( apiVersion );

        if ( request.RequestUri is not Uri url ||
             url.Query.Contains( parameterName, OrdinalIgnoreCase ) )
        {
            return;
        }

        var builder = new UriBuilder( url );

        if ( !string.IsNullOrEmpty( builder.Query ) && builder.Query.Length > 1 )
        {
            builder.Query += '&';
        }

        builder.Query += $"{parameterName}={apiVersion}";
        request.RequestUri = builder.Uri;
    }
}