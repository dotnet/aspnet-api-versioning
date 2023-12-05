// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if !NETFRAMEWORK
using System.Buffers;
#endif
using static Asp.Versioning.ApiVersionParameterLocation;
using static System.StringComparer;

/// <summary>
/// Represents an API version reader that reads the value from the query string in a URL.
/// </summary>
public partial class QueryStringApiVersionReader : IApiVersionReader
{
    private const string DefaultQueryParameterName = "api-version";

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryStringApiVersionReader"/> class.
    /// </summary>
    /// <remarks>This constructor always adds the "api-version" query string parameter.</remarks>
    public QueryStringApiVersionReader() => ParameterNames.Add( DefaultQueryParameterName );

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryStringApiVersionReader"/> class.
    /// </summary>
    /// <param name="parameterNames">A <see cref="IEnumerable{T}">sequence</see> of query string parameter names to read the API version from.</param>
    /// <remarks>This constructor adds the "api-version" query string parameter if no other query parameter names are specified.</remarks>
    public QueryStringApiVersionReader( IEnumerable<string> parameterNames )
    {
        ArgumentNullException.ThrowIfNull( parameterNames );

        ParameterNames.AddRange( parameterNames );

        if ( ParameterNames.Count == 0 )
        {
            ParameterNames.Add( DefaultQueryParameterName );
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryStringApiVersionReader"/> class.
    /// </summary>
    /// <param name="parameterName">The primary query string parameter name to read the API version from.</param>
    /// <param name="otherParameterNames">An array of query string parameter names to read the API version from.</param>
    public QueryStringApiVersionReader( string parameterName, params string[] otherParameterNames )
    {
        ArgumentException.ThrowIfNullOrEmpty( parameterName );

        ParameterNames.Add( parameterName );

        if ( otherParameterNames is not null )
        {
            for ( var i = 0; i < otherParameterNames.Length; i++ )
            {
                var name = otherParameterNames[i];

                if ( !string.IsNullOrEmpty( name ) )
                {
                    ParameterNames.Add( name );
                }
            }
        }
    }

    /// <summary>
    /// Gets a collection of HTTP header names that the API version can be read from.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of HTTP header names.</value>
    /// <remarks>HTTP header names are evaluated in a case-insensitive manner.</remarks>
    public ICollection<string> ParameterNames { get; } = new HashSet<string>( OrdinalIgnoreCase );

    /// <summary>
    /// Provides API version parameter descriptions supported by the current reader using the supplied provider.
    /// </summary>
    /// <param name="context">The <see cref="IApiVersionParameterDescriptionContext">context</see> used to add API version parameter descriptions.</param>
    public virtual void AddParameters( IApiVersionParameterDescriptionContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var count = ParameterNames.Count;
#if NETFRAMEWORK
        var names = new string[count];
#else
        var pool = ArrayPool<string>.Shared;
        var names = pool.Rent( count );
#endif

        ParameterNames.CopyTo( names, 0 );

        for ( var i = 0; i < count; i++ )
        {
            context.AddParameter( names[i], Query );
        }

#if !NETFRAMEWORK
        pool.Return( names );
#endif
    }
}