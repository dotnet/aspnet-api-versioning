// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System.Web.Http.Routing;

/// <summary>
/// Represents a route constraint for <see cref="ApiVersion">API versions</see>.
/// </summary>
public sealed class ApiVersionRouteConstraint : IHttpRouteConstraint
{
    /// <summary>
    /// Determines whether the route constraint matches the specified criteria.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
    /// <param name="route">The current <see cref="IHttpRoute">route</see>.</param>
    /// <param name="parameterName">The parameter name to match.</param>
    /// <param name="values">The current <see cref="IDictionary{TKey, TValue}">collection</see> of route values.</param>
    /// <param name="routeDirection">The <see cref="HttpRouteDirection">route direction</see> to match.</param>
    /// <returns>True if the route constraint is matched; otherwise, false.</returns>
    public bool Match( HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object?> values, HttpRouteDirection routeDirection )
    {
        ArgumentNullException.ThrowIfNull( values );

        if ( string.IsNullOrEmpty( parameterName ) )
        {
            return false;
        }

        if ( !values.TryGetValue( parameterName, out string? value ) )
        {
            return false;
        }

        if ( routeDirection == HttpRouteDirection.UriGeneration )
        {
            return !string.IsNullOrEmpty( value );
        }

        var parser = request.GetConfiguration().GetApiVersionParser();
        var properties = request.ApiVersionProperties();

        properties.RouteParameter = parameterName;
        properties.RawRequestedApiVersion = value;

        if ( parser.TryParse( value, out var requestedVersion ) )
        {
            properties.RequestedApiVersion = requestedVersion;
            return true;
        }

        return false;
    }
}