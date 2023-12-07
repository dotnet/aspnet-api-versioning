// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Globalization;
#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ApiExplorer;
#endif
#if NETFRAMEWORK
using System.Web.Http.Controllers;
using System.Web.Http.Description;
#endif

/// <summary>
/// Represents an OData query options convention builder.
/// </summary>
public partial class ODataQueryOptionsConventionBuilder
{
    private IODataQueryOptionDescriptionProvider? descriptionProvider;
    private Dictionary<Type, IODataQueryOptionsConventionBuilder>? conventionBuilders;
    private List<IODataQueryOptionsConvention>? conventions;

    /// <summary>
    /// Gets or sets the provider used to describe query options.
    /// </summary>
    /// <value>The <see cref="IODataQueryOptionDescriptionProvider">provider</see> used to describe OData query options.</value>
    public IODataQueryOptionDescriptionProvider DescriptionProvider
    {
        get => descriptionProvider ??= new DefaultODataQueryOptionDescriptionProvider();
        set => descriptionProvider = value;
    }

    /// <summary>
    /// Gets the count of configured conventions.
    /// </summary>
    /// <value>The total count of configured conventions.</value>
    public virtual int Count =>
        ( conventionBuilders is null ? 0 : conventionBuilders.Count ) +
        ( conventions is null ? 0 : conventions.Count );

    /// <summary>
    /// Gets a collection of controller convention builders.
    /// </summary>
    /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of
    /// <see cref="IODataQueryOptionsConventionBuilder">controller convention builders</see>.</value>
    protected IDictionary<Type, IODataQueryOptionsConventionBuilder> ConventionBuilders => conventionBuilders ??= [];

    /// <summary>
    /// Gets a collection of OData query option conventions.
    /// </summary>
    /// <value>A <see cref="IList{T}">list</see> of <see cref="IODataQueryOptionsConvention">OData query option conventions</see>.</value>
    protected IList<IODataQueryOptionsConvention> Conventions => conventions ??= [];

    /// <summary>
    /// Gets or creates the convention builder for the specified controller.
    /// </summary>
    /// <typeparam name="TController">The <see cref="Type">type</see> of controller to build conventions for.</typeparam>
    /// <returns>A new or existing <see cref="ODataControllerQueryOptionsConventionBuilder{T}"/>.</returns>
    public virtual ODataControllerQueryOptionsConventionBuilder<TController> Controller<TController>()
        where TController : notnull
#if NETFRAMEWORK
#pragma warning disable IDE0079
#pragma warning disable SA1001 // Commas should be spaced correctly
       , IHttpController
#pragma warning restore SA1001 // Commas should be spaced correctly
#pragma warning restore IDE0079
#endif
    {
        var key = typeof( TController );

        if ( !ConventionBuilders.TryGetValue( key, out var builder ) )
        {
            var newBuilder = new ODataControllerQueryOptionsConventionBuilder<TController>();
            ConventionBuilders[key] = newBuilder;
            return newBuilder;
        }

        if ( builder is ODataControllerQueryOptionsConventionBuilder<TController> typedBuilder )
        {
            return typedBuilder;
        }

        var message = string.Format( CultureInfo.CurrentCulture, Format.ConventionStyleMismatch, key.Name );
        throw new InvalidOperationException( message );
    }

    /// <summary>
    /// Gets or creates the convention builder for the specified controller.
    /// </summary>
    /// <param name="controllerType">The <see cref="Type">type</see> of controller to build conventions for.</param>
    /// <returns>A new or existing <see cref="ODataControllerQueryOptionsConventionBuilder"/>.</returns>
    public virtual ODataControllerQueryOptionsConventionBuilder Controller( Type controllerType )
    {
        ArgumentNullException.ThrowIfNull( controllerType );

        if ( !ConventionBuilders.TryGetValue( controllerType, out var builder ) )
        {
            var newBuilder = new ODataControllerQueryOptionsConventionBuilder( controllerType );
            ConventionBuilders[controllerType] = newBuilder;
            return newBuilder;
        }

        if ( builder is ODataControllerQueryOptionsConventionBuilder typedBuilder )
        {
            return typedBuilder;
        }

        var message = string.Format( CultureInfo.CurrentCulture, Format.ConventionStyleMismatch, controllerType.Name );
        throw new InvalidOperationException( message );
    }

    /// <summary>
    /// Adds a new OData query option convention.
    /// </summary>
    /// <param name="convention">The <see cref="IODataQueryOptionsConvention">convention</see> to be applied.</param>
    public virtual void Add( IODataQueryOptionsConvention convention ) => Conventions.Add( convention );

    /// <summary>
    /// Applies the defined OData query option conventions to the specified API description.
    /// </summary>
    /// <param name="apiDescriptions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiDescription">API descriptions</see>
    /// to apply configured conventions to.</param>
    /// <param name="queryOptionSettings">The <see cref="ODataQueryOptionSettings">settings</see> used to apply OData query option conventions.</param>
    public virtual void ApplyTo( IEnumerable<ApiDescription> apiDescriptions, ODataQueryOptionSettings queryOptionSettings )
    {
        ArgumentNullException.ThrowIfNull( apiDescriptions );

        var controllerConventions = default( Dictionary<Type, IODataQueryOptionsConvention> );

        foreach ( var description in apiDescriptions )
        {
            var controller = GetController( description );

            if ( !controller.IsODataController() && !description.IsODataLike() )
            {
                continue;
            }

            if ( controllerConventions == null || !controllerConventions.TryGetValue( controller, out var convention ) )
            {
                if ( conventionBuilders == null ||
                     conventionBuilders.Count == 0 ||
                    !conventionBuilders.TryGetValue( controller, out var builder ) )
                {
                    builder = new ODataControllerQueryOptionsConventionBuilder( controller );
                }

                convention = builder.Build( queryOptionSettings );
                controllerConventions ??= [];
                controllerConventions.Add( controller, convention );
            }

            convention.ApplyTo( description );

            for ( var i = 0; i < Conventions.Count; i++ )
            {
                Conventions[i].ApplyTo( description );
            }
        }
    }
}