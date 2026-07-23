// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Http;

using Asp.Versioning;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.ComponentModel;
using RoutePattern = Microsoft.AspNetCore.Routing.Patterns.RoutePattern;

/// <summary>
/// Provides extension methods for <see cref="HttpRequest"/>.
/// </summary>
[CLSCompliant( false )]
public static class HttpRequestExtensions
{
    /// <param name="request">The current <see cref="HttpRequest">HTTP request</see>.</param>
    extension( HttpRequest request )
    {
        /// <summary>
        /// Attempts to get the API version from current request path using the provided patterns.
        /// </summary>
        /// <typeparam name="TList">The type of <see cref="IReadOnlyList{T}">read-only list</see>.</typeparam>
        /// <param name="routePatterns">The <see cref="IReadOnlyList{T}">read-only list</see> of
        /// <see cref="RoutePattern">patterns</see> to evaluate.</param>
        /// <param name="constraintName">The name of the API version route constraint.</param>
        /// <param name="apiVersion">The raw API version, if retrieved.</param>
        /// <returns>True if the raw API version was retrieved; otherwise, false.</returns>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public bool TryGetApiVersionFromPath<TList>(
            TList routePatterns,
            string constraintName,
            [NotNullWhen( true )] out string? apiVersion )
            where TList : IReadOnlyList<RoutePattern>
        {
            ArgumentNullException.ThrowIfNull( request );
            ArgumentNullException.ThrowIfNull( routePatterns );

            if ( string.IsNullOrEmpty( constraintName ) || routePatterns.Count == 0 )
            {
                return request.TryInferApiVersionFromSegment( out apiVersion );
            }

            var path = request.Path;
            var values = default( RouteValueDictionary );

            // this only applies when versioning by url segment. route values have not been processed
            // since no candidates exist yet. we do know the name of the route constraint though. there
            // is only one constraint that applies to the api version so we can use that to extract
            // the api version from any suitable route template. we're not matching the route template,
            // just the raw api version since we don't have a collection of route values to work with.
            for ( var i = 0; i < routePatterns.Count; i++ )
            {
                var routePattern = routePatterns[i];
                var defaults = new RouteValueDictionary( routePattern.RequiredValues );
                var matcher = new TemplateMatcher( new( routePattern ), defaults );

                if ( values is null )
                {
                    values = [];
                }
                else
                {
                    values.Clear();
                }

                if ( !matcher.TryMatch( path, values ) )
                {
                    continue;
                }

                var parameters = routePattern.Parameters;

                for ( var j = 0; j < parameters.Count; j++ )
                {
                    var parameter = parameters[j];
                    var policies = parameter.ParameterPolicies;

                    for ( var k = 0; k < policies.Count; k++ )
                    {
                        if ( constraintName.Equals( policies[k].Content, StringComparison.Ordinal ) &&
                             values.TryGetValue( parameter.Name, out apiVersion ) &&
                             !string.IsNullOrEmpty( apiVersion ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return request.TryInferApiVersionFromSegment( out apiVersion );
        }

        /// <summary>
        /// Attempts to infer the API version from the current request path by looking for at each segment.
        /// </summary>
        /// <param name="apiVersion">The raw API version, if retrieved.</param>
        /// <returns>True if the raw API version was retrieved; otherwise, false.</returns>
        /// <remarks>
        /// <para>
        /// This is the last resort for inferring the API version and is intrinsically slow. There are no route
        /// constraints to match against. '/v1' and 'api/v1' are the only recognized and supported prefixes as
        /// versioning by URL segment is uniformly at the beginning of the path and a later segment in the path could
        /// be accidentally misinterpreted.
        /// </para>
        /// <para>This approach addresses at least two use cases when an API version is specified as a segment in the
        /// URL path:
        /// <list type="bullet">
        /// <item>
        /// <description>with a status</description>
        /// </item>
        /// <item>
        /// <description>using gRPC</description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// An alternate design could support configuring a route template for the purposes of matching, but that would
        /// only address the gRPC scenario. The path 'v2-preview' or 'api/v2-preview' would still not match.
        /// </para>
        /// </remarks>
        private bool TryInferApiVersionFromSegment( [NotNullWhen( true )] out string? apiVersion )
        {
            if ( request.HttpContext.RequestServices.GetService<IApiVersionParser>() is not { } parser )
            {
                apiVersion = default;
                return false;
            }

            var segments = new StringTokenizer( request.Path, ['/'] );
            var count = 0;

            foreach ( var segment in segments )
            {
                switch ( segment.Length )
                {
                    case 0:
                        continue;
                    case 1:
                        goto NoMatch;
                    default:
                        ++count;
                        break;
                }

                if ( count > 2 )
                {
                    break;
                }

                var span = segment.AsSpan();

                if ( span[0] == 'v' || span[0] == 'V' )
                {
                    span = span[1..];

                    if ( parser.TryParse( span, out var _ ) )
                    {
                        apiVersion = span.ToString();
                        return true;
                    }
                }
            }

        NoMatch:

            apiVersion = default;
            return false;
        }
    }
}