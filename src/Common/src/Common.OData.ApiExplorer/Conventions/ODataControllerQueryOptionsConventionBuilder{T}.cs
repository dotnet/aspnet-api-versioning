// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.ComponentModel;
using System.Reflection;
#if NETFRAMEWORK
using System.Web.Http.Controllers;
#endif

/// <summary>
/// Represents an OData query options convention builder.
/// </summary>
/// <typeparam name="T">The type of controller.</typeparam>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public class ODataControllerQueryOptionsConventionBuilder<T> :
    IODataQueryOptionsConventionBuilder,
    IODataActionQueryOptionsConventionBuilder<T>
    where T : notnull
#if NETFRAMEWORK
#pragma warning disable IDE0079
#pragma warning disable SA1001 // Commas should be spaced correctly
    , IHttpController
#pragma warning restore SA1001 // Commas should be spaced correctly
#pragma warning restore IDE0079
#endif
{
    private ODataActionQueryOptionsConventionBuilderCollection<T>? actionBuilders;

    /// <summary>
    /// Gets a collection of controller action convention builders.
    /// </summary>
    /// <value>A <see cref="ODataActionQueryOptionsConventionBuilderCollection{T}">collection</see> of
    /// <see cref="ODataActionQueryOptionsConventionBuilder{T}">controller action convention builders</see>.</value>
    protected virtual ODataActionQueryOptionsConventionBuilderCollection<T> ActionBuilders => actionBuilders ??= new( this );

    /// <summary>
    /// Gets or creates the convention builder for the specified controller action method.
    /// </summary>
    /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
    /// <returns>A new or existing <see cref="ODataActionQueryOptionsConventionBuilder{T}"/>.</returns>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public virtual ODataActionQueryOptionsConventionBuilder<T> Action( MethodInfo actionMethod ) => ActionBuilders.GetOrAdd( actionMethod );

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