// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
#pragma warning disable IDE0079
using Microsoft.AspNetCore.Mvc.ApplicationModels;
#endif
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
#if NETFRAMEWORK
using System.Web.Http.Controllers;
using ActionModel = System.Web.Http.Controllers.HttpActionDescriptor;
#endif

/// <summary>
/// Represents a builder for API versions applied to a controller.
/// </summary>
/// <typeparam name="T">The type of item the convention builder is for.</typeparam>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public class ControllerApiVersionConventionBuilder<T> :
    ControllerApiVersionConventionBuilderBase,
    IControllerConventionBuilder,
    IControllerConventionBuilder<T>
#if NETFRAMEWORK
    where T : notnull, IHttpController
#else
    where T : notnull
#endif
{
    private ActionApiVersionConventionBuilderCollection<T>? actionBuilders;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilder{T}"/> class.
    /// </summary>
    public ControllerApiVersionConventionBuilder() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilder{T}"/> class.
    /// </summary>
    /// <param name="namingConvention">The <see cref="IControllerNameConvention">controller naming convention</see>.</param>
    public ControllerApiVersionConventionBuilder( IControllerNameConvention namingConvention ) : base( namingConvention ) { }

    /// <summary>
    /// Gets a collection of controller action convention builders.
    /// </summary>
    /// <value>A <see cref="ActionApiVersionConventionBuilderCollection{T}">collection</see> of
    /// <see cref="ActionApiVersionConventionBuilder{T}">controller action convention builders</see>.</value>
    protected virtual ActionApiVersionConventionBuilderCollection<T> ActionBuilders => actionBuilders ??= new( this );

    /// <summary>
    /// Indicates that the controller is API version-neutral.
    /// </summary>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder<T> IsApiVersionNeutral()
    {
        VersionNeutral = true;
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is supported by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the controller.</param>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder<T> HasApiVersion( ApiVersion apiVersion )
    {
        SupportedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the controller.</param>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder<T> HasDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder<T> AdvertisesApiVersion( ApiVersion apiVersion )
    {
        AdvertisedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder<T> AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedAdvertisedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Gets or creates the convention builder for the specified controller action method.
    /// </summary>
    /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
    /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
    /// <remarks>This API is meant for infrastructure and should not be used by application code.</remarks>
    [EditorBrowsable( EditorBrowsableState.Never )]
    public virtual ActionApiVersionConventionBuilder<T> Action( MethodInfo actionMethod ) => ActionBuilders.GetOrAdd( actionMethod );

    /// <summary>
    /// Attempts to get the convention for the specified action method.
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo">method</see> representing the action to retrieve the convention for.</param>
    /// <param name="convention">The retrieved <see cref="IApiVersionConvention{T}">convention</see> or <c>null</c>.</param>
    /// <returns>True if the convention was successfully retrieved; otherwise, false.</returns>
    protected override bool TryGetConvention( MethodInfo method, [MaybeNullWhen( false )] out IApiVersionConvention<ActionModel> convention )
    {
        if ( actionBuilders is not null && actionBuilders.TryGetValue( method, out var builder ) )
        {
            return ( convention = builder ) is not null;
        }

        convention = default!;
        return false;
    }

#pragma warning disable IDE0079
#pragma warning disable CA1033 // Interface methods should be callable by child types
    Type IControllerConventionBuilder.ControllerType => typeof( T );
#pragma warning restore CA1033 // Interface methods should be callable by child types
#pragma warning restore IDE0079

    void IDeclareApiVersionConventionBuilder.IsApiVersionNeutral() => IsApiVersionNeutral();

    void IDeclareApiVersionConventionBuilder.HasApiVersion( ApiVersion apiVersion ) => HasApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.HasDeprecatedApiVersion( ApiVersion apiVersion ) => HasDeprecatedApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesApiVersion( ApiVersion apiVersion ) => AdvertisesApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) => AdvertisesDeprecatedApiVersion( apiVersion );

    IActionConventionBuilder IControllerConventionBuilder.Action( MethodInfo actionMethod ) => Action( actionMethod );

    IActionConventionBuilder<T> IControllerConventionBuilder<T>.Action( MethodInfo actionMethod ) => Action( actionMethod );
}