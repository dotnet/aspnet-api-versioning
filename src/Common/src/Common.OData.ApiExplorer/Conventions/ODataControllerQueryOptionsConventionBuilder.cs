// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.ComponentModel;
using System.Globalization;
using System.Reflection;

/// <summary>
/// Represents an OData query options convention builder.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public partial class ODataControllerQueryOptionsConventionBuilder : IODataQueryOptionsConventionBuilder, IODataActionQueryOptionsConventionBuilder
{
    private ODataActionQueryOptionsConventionBuilderCollection? actionBuilders;

    /// <summary>
    /// Initializes a new instance of the <see cref="ODataControllerQueryOptionsConventionBuilder"/> class.
    /// </summary>
    /// <param name="controllerType">The <see cref="Type">type</see> of controller the convention builder is for.</param>
    public ODataControllerQueryOptionsConventionBuilder( Type controllerType )
    {
#if NETFRAMEWORK
        var webApiController = typeof( System.Web.Http.Controllers.IHttpController );

        if ( !webApiController.IsAssignableFrom( controllerType ) )
        {
            var message = string.Format( CultureInfo.CurrentCulture, Format.RequiredInterfaceNotImplemented, controllerType, webApiController );
            throw new System.ArgumentException( message );
        }
#endif
        ControllerType = controllerType;
    }

    /// <summary>
    /// Gets the type of controller the convention builder is for.
    /// </summary>
    /// <value>The corresponding controller <see cref="Type">type</see>.</value>
    public Type ControllerType { get; }

    /// <summary>
    /// Gets a collection of controller action convention builders.
    /// </summary>
    /// <value>A <see cref="ODataActionQueryOptionsConventionBuilderCollection">collection</see> of
    /// <see cref="ODataActionQueryOptionsConventionBuilder">controller action convention builders</see>.</value>
    protected internal virtual ODataActionQueryOptionsConventionBuilderCollection ActionBuilders => actionBuilders ??= new( this );

    /// <summary>
    /// Gets or creates the convention builder for the specified controller action method.
    /// </summary>
    /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
    /// <returns>A new or existing <see cref="ODataActionQueryOptionsConventionBuilder"/>.</returns>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public virtual ODataActionQueryOptionsConventionBuilder Action( MethodInfo actionMethod ) => ActionBuilders.GetOrAdd( actionMethod );

    /// <inheritdoc />
    public virtual IODataQueryOptionsConvention Build( ODataQueryOptionSettings settings ) =>
        new ODataControllerQueryOptionConvention( Lookup, settings );

    private bool Lookup( MethodInfo action, ODataQueryOptionSettings settings, out IODataQueryOptionsConvention? convention )
    {
        if ( actionBuilders == null || actionBuilders.Count == 0 )
        {
            convention = default;
            return false;
        }

        if ( actionBuilders.TryGetValue( action, out var builder ) )
        {
            convention = builder!.Build( settings );
            return true;
        }

        convention = default;
        return false;
    }
}