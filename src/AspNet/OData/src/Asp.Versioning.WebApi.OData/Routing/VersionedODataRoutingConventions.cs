// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNet.OData.Routing.Conventions;
using System.Web.Http;

/// <content>
/// Provides utility functions to create OData routing conventions with support for API versioning.
/// </content>
public static partial class VersionedODataRoutingConventions
{
    /// <summary>
    /// Creates a mutable list of the default OData routing conventions with attribute routing enabled.
    /// </summary>
    /// <param name="routeName">The name of the route.</param>
    /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
    /// <returns>A mutable list of the default OData routing conventions.</returns>
    public static IList<IODataRoutingConvention> CreateDefaultWithAttributeRouting(
        string routeName,
        HttpConfiguration configuration ) =>
        EnsureConventions(
            ODataRoutingConventions.CreateDefault(),
            new VersionedAttributeRoutingConvention( routeName, configuration ) );

    /// <summary>
    /// Creates a mutable list of the default OData routing conventions with support for API versioning.
    /// </summary>
    /// <returns>A mutable <see cref="IList{T}">list</see> of the default <see cref="IODataRoutingConvention">OData routing conventions</see>.</returns>
    public static IList<IODataRoutingConvention> CreateDefault() => EnsureConventions( ODataRoutingConventions.CreateDefault() );

    /// <summary>
    /// Adds or updates the specified list of OData routing conventions to ensure the necessary API versioning conventions are present.
    /// </summary>
    /// <param name="routingConventions">The <see cref="IList{T}"/> of <see cref="IODataRoutingConvention">OData routing conventions</see> to add to or update.</param>
    /// <returns>The original <see cref="IList{T}">list</see> of <see cref="IODataRoutingConvention">OData routing conventions</see>,
    /// possibly with new or updated <see cref="IODataRoutingConvention">OData routing conventions</see>.</returns>
    public static IList<IODataRoutingConvention> AddOrUpdate( IList<IODataRoutingConvention> routingConventions )
    {
        if ( routingConventions == null )
        {
            throw new ArgumentNullException( nameof( routingConventions ) );
        }

        return EnsureConventions( routingConventions );
    }

    private static IList<IODataRoutingConvention> EnsureConventions(
        IList<IODataRoutingConvention> conventions,
        VersionedAttributeRoutingConvention? attributeRoutingConvention = default )
    {
        var hasVersionedAttributeConvention = false;
        var hasVersionedMetadataConvention = false;

        for ( var i = conventions.Count - 1; i >= 0; i-- )
        {
            var convention = conventions[i];

            if ( convention is AttributeRoutingConvention &&
                 convention.GetType().Equals( typeof( AttributeRoutingConvention ) ) )
            {
                if ( attributeRoutingConvention == default )
                {
                    conventions.RemoveAt( i );
                }
                else
                {
                    conventions[i] = attributeRoutingConvention;
                    hasVersionedAttributeConvention = true;
                }
            }
            else if ( convention is MetadataRoutingConvention )
            {
                conventions[i] = new VersionedMetadataRoutingConvention();
                hasVersionedMetadataConvention = true;
            }
            else if ( convention is VersionedMetadataRoutingConvention )
            {
                hasVersionedMetadataConvention = true;
            }
        }

        if ( !hasVersionedMetadataConvention )
        {
            conventions.Insert( 0, new VersionedMetadataRoutingConvention() );
        }

        if ( !hasVersionedAttributeConvention && attributeRoutingConvention != default )
        {
            conventions.Insert( 0, attributeRoutingConvention );
        }

        return conventions;
    }
}