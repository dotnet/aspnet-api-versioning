// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ApplicationModels;
#endif
using System.Globalization;
#if NETFRAMEWORK
using System.Web.Http.Controllers;
using ControllerModel = System.Web.Http.Controllers.HttpControllerDescriptor;
#endif

/// <summary>
/// Represents an object used to configure and create API version conventions for a controllers and their actions.
/// </summary>
public partial class ApiVersionConventionBuilder : IApiVersionConventionBuilder
{
    private Dictionary<Type, IControllerConventionBuilder>? controllerConventionBuilders;
    private List<IControllerConvention>? controllerConventions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionConventionBuilder"/> class.
    /// </summary>
    public ApiVersionConventionBuilder() => NamingConvention = ControllerNameConvention.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionConventionBuilder"/> class.
    /// </summary>
    /// <param name="namingConvention">The <see cref="IControllerNameConvention">controller naming convention</see>.</param>
    public ApiVersionConventionBuilder( IControllerNameConvention namingConvention ) => NamingConvention = namingConvention;

    /// <summary>
    /// Gets the controller naming convention associated with the builder.
    /// </summary>
    /// <value>The <see cref="IControllerNameConvention">controller naming convention</see>.</value>
    protected IControllerNameConvention NamingConvention { get; }

    /// <summary>
    /// Gets a collection of controller convention builders.
    /// </summary>
    /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of <see cref="IControllerConventionBuilder">controller convention builders</see>.</value>
    protected IDictionary<Type, IControllerConventionBuilder> ControllerConventionBuilders => controllerConventionBuilders ??= [];

    /// <summary>
    /// Gets a collection of controller conventions.
    /// </summary>
    /// <value>A <see cref="IList{T}">list</see> of <see cref="IControllerConvention">controller conventions</see>.</value>
    protected IList<IControllerConvention> ControllerConventions => controllerConventions ??= [];

    /// <inheritdoc />
    public virtual int Count =>
        ( controllerConventionBuilders is null ? 0 : controllerConventionBuilders.Count ) +
        ( controllerConventions is null ? 0 : controllerConventions.Count );

    /// <inheritdoc />
    public virtual IControllerConventionBuilder<TController> Controller<TController>()
#if NETFRAMEWORK
        where TController : notnull, IHttpController
#else
        where TController : notnull
#endif
    {
        var key = typeof( TController );

        if ( !ControllerConventionBuilders.TryGetValue( key, out var builder ) )
        {
            var newBuilder = new ControllerApiVersionConventionBuilder<TController>( NamingConvention );
            ControllerConventionBuilders[key] = newBuilder;
            return newBuilder;
        }

        if ( builder is IControllerConventionBuilder<TController> typedBuilder )
        {
            return typedBuilder;
        }

        // this should only ever happen if a subclass overrides Controller(Type) and adds a
        // IControllerConventionBuilder that is not covariant with IControllerConventionBuilder<TController>
        var message = string.Format( CultureInfo.CurrentCulture, MvcFormat.ConventionStyleMismatch, key.Name );
        throw new InvalidOperationException( message );
    }

    /// <inheritdoc />
    public virtual IControllerConventionBuilder Controller( Type controllerType )
    {
        if ( !ControllerConventionBuilders.TryGetValue( controllerType, out var builder ) )
        {
            var newBuilder = NewGenericControllerConventionBuilder( controllerType, NamingConvention );
            ControllerConventionBuilders[controllerType] = newBuilder;
            return newBuilder;
        }

        return builder;
    }

    /// <inheritdoc />
    public virtual void Add( IControllerConvention convention ) => ControllerConventions.Add( convention );

    /// <inheritdoc />
    public virtual bool ApplyTo( ControllerModel controller )
    {
        ArgumentNullException.ThrowIfNull( controller );

        IControllerConventionBuilder? builder;
        bool hasExplicitConventions;

        if ( controllerConventionBuilders is null )
        {
            hasExplicitConventions = false;
            builder = default;
        }
        else
        {
            hasExplicitConventions = controllerConventionBuilders.TryGetValue( controller.ControllerType, out builder );
        }

        var applied = hasExplicitConventions;

        if ( !hasExplicitConventions )
        {
            var hasNoExplicitConventions = controllerConventions is null || controllerConventions.Count == 0;

            if ( hasNoExplicitConventions && !( applied = HasDecoratedActions( controller ) ) )
            {
                return false;
            }

            builder = new ControllerApiVersionConventionBuilder( controller.ControllerType, NamingConvention );
        }

        if ( controllerConventions is not null )
        {
            for ( var i = 0; i < controllerConventions.Count; i++ )
            {
                applied |= controllerConventions[i].Apply( builder!, controller );
            }
        }

        if ( applied )
        {
            builder!.ApplyTo( controller );
        }

        return applied;
    }

    private static IControllerConventionBuilder NewGenericControllerConventionBuilder(
        Type controllerType,
        IControllerNameConvention namingConvention )
    {
        // since this only happens once per controller type, there's no advantage to compiling
        // or caching a strongly-typed activator function
        var builderOfT = typeof( ControllerApiVersionConventionBuilder<> );
        var builder = builderOfT.MakeGenericType( controllerType );
        return (IControllerConventionBuilder) Activator.CreateInstance( builder, namingConvention )!;
    }
}