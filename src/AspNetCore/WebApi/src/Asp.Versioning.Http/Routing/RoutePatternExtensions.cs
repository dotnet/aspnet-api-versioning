// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System.ComponentModel;
using RoutePattern = Microsoft.AspNetCore.Routing.Patterns.RoutePattern;

/// <summary>
/// Provides extension methods for <see cref="RoutePattern"/>.
/// </summary>
[CLSCompliant( false )]
[EditorBrowsable( EditorBrowsableState.Never )]
public static class RoutePatternExtensions
{
    /// <param name="routePattern">The <see cref="RoutePattern">route pattern</see> to evaluate.</param>
    extension( RoutePattern routePattern )
    {
        /// <summary>
        /// Determines whether the route pattern contains the specified constraint name.
        /// </summary>
        /// <param name="constraintName">The name of the constraint to find.</param>
        /// <returns>True if the route pattern has the <paramref name="constraintName"/>; otherwise, false.</returns>
        public bool HasVersionConstraint( string constraintName )
        {
            ArgumentNullException.ThrowIfNull( routePattern );

            if ( string.IsNullOrEmpty( constraintName ) )
            {
                return false;
            }

            var parameters = routePattern.Parameters;

            for ( var i = 0; i < parameters.Count; i++ )
            {
                var parameter = parameters[i];
                var policies = parameter.ParameterPolicies;

                for ( var j = 0; j < policies.Count; j++ )
                {
                    if ( constraintName.Equals( policies[j].Content, StringComparison.Ordinal ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}