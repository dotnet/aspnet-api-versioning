// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using System.ComponentModel;
using RoutePattern = Microsoft.AspNetCore.Routing.Patterns.RoutePattern;

/// <summary>
/// Provides extension methods for <see cref="HttpRequest"/>.
/// </summary>
[CLSCompliant( false )]
public static class HttpRequestExtensions
{
    /// <summary>
    /// Attempts to get the API version from current request path using the provided patterns.
    /// </summary>
    /// <typeparam name="TList">The type of <see cref="IReadOnlyList{T}">read-only list</see>.</typeparam>
    /// <param name="request">The current <see cref="HttpRequest">HTTP request</see>.</param>
    /// <param name="routePatterns">The <see cref="IReadOnlyList{T}">read-only list</see> of
    /// <see cref="RoutePattern">patterns</see> to evaluate.</param>
    /// <param name="constraintName">The name of the API version route constraint.</param>
    /// <param name="apiVersion">The raw API version, if retrieved.</param>
    /// <returns>True if the raw API version was retrieved; otherwise, false.</returns>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static bool TryGetApiVersionFromPath<TList>(
        this HttpRequest request,
        TList routePatterns,
        string constraintName,
        [NotNullWhen( true )] out string? apiVersion )
        where TList : IReadOnlyList<RoutePattern>
    {
        ArgumentNullException.ThrowIfNull( routePatterns );

        if ( string.IsNullOrEmpty( constraintName ) || routePatterns.Count == 0 )
        {
            apiVersion = default;
            return false;
        }

        var path = ( request ?? throw new ArgumentNullException( nameof( request ) ) ).Path;
        var values = new RouteValueDictionary();

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

            values.Clear();

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

        apiVersion = default;
        return false;
    }
}