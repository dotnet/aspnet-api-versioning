// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc.ApplicationModels;
#endif
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
#if NETFRAMEWORK
using System.Web.Http.Controllers;
using ActionModel = System.Web.Http.Controllers.HttpActionDescriptor;
#endif

/// <summary>
/// Represents a builder for API versions applied to a controller.
/// </summary>
#if !NETFRAMEWORK
[CLSCompliant( false )]
#endif
public class ControllerApiVersionConventionBuilder : ControllerApiVersionConventionBuilderBase, IControllerConventionBuilder
{
    private ActionApiVersionConventionBuilderCollection? actionBuilders;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilder"/> class.
    /// </summary>
    /// <param name="controllerType">The <see cref="Type">type</see> of controller the convention builder is for.</param>
    public ControllerApiVersionConventionBuilder( Type controllerType )
        : this( controllerType, ControllerNameConvention.Default ) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilder"/> class.
    /// </summary>
    /// <param name="controllerType">The <see cref="Type">type</see> of controller the convention builder is for.</param>
    /// <param name="namingConvention">The <see cref="IControllerNameConvention">controller naming convention</see>.</param>
    public ControllerApiVersionConventionBuilder( Type controllerType, IControllerNameConvention namingConvention )
        : base( namingConvention )
    {
#if NETFRAMEWORK
        var webApiController = typeof( IHttpController );

        if ( !webApiController.IsAssignableFrom( controllerType ) )
        {
            var message = string.Format( CultureInfo.CurrentCulture, MvcSR.RequiredInterfaceNotImplemented, controllerType, webApiController );
            throw new System.ArgumentException( message, nameof( controllerType ) );
        }
#endif
        ControllerType = controllerType;
    }

    /// <inheritdoc />
    public Type ControllerType { get; }

    /// <summary>
    /// Gets a collection of controller action convention builders.
    /// </summary>
    /// <value>A <see cref="ActionApiVersionConventionBuilderCollection">collection</see> of
    /// <see cref="ActionApiVersionConventionBuilder">controller action convention builders</see>.</value>
    protected virtual ActionApiVersionConventionBuilderCollection ActionBuilders => actionBuilders ??= new( this );

    /// <summary>
    /// Indicates that the controller is API version-neutral.
    /// </summary>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder IsApiVersionNeutral()
    {
        VersionNeutral = true;
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is supported by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the controller.</param>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder HasApiVersion( ApiVersion apiVersion )
    {
        SupportedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is deprecated by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the controller.</param>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder AdvertisesApiVersion( ApiVersion apiVersion )
    {
        AdvertisedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Indicates that the specified API version is advertised and deprecated by the configured controller.
    /// </summary>
    /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
    /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
    public virtual ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
    {
        DeprecatedAdvertisedVersions.Add( apiVersion );
        return this;
    }

    /// <summary>
    /// Gets or creates the convention builder for the specified controller action method.
    /// </summary>
    /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
    /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder"/>.</returns>
    public virtual ActionApiVersionConventionBuilder Action( MethodInfo actionMethod ) => ActionBuilders.GetOrAdd( actionMethod );

    /// <summary>
    /// Attempts to get the convention for the specified action method.
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo">method</see> representing the action to retrieve the convention for.</param>
    /// <param name="convention">The retrieved <see cref="IApiVersionConvention{T}">convention</see> or <c>null</c>.</param>
    /// <returns>True if the convention was successfully retrieved; otherwise, false.</returns>
    protected override bool TryGetConvention( MethodInfo method, [MaybeNullWhen( false )] out IApiVersionConvention<ActionModel> convention )
    {
        if ( actionBuilders is not null &&
             actionBuilders.TryGetValue( method, out var actionBuilder ) )
        {
            return ( convention = actionBuilder ) is not null;
        }

        convention = default!;
        return false;
    }

    void IDeclareApiVersionConventionBuilder.IsApiVersionNeutral() => IsApiVersionNeutral();

    void IDeclareApiVersionConventionBuilder.HasApiVersion( ApiVersion apiVersion ) => HasApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.HasDeprecatedApiVersion( ApiVersion apiVersion ) => HasDeprecatedApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesApiVersion( ApiVersion apiVersion ) => AdvertisesApiVersion( apiVersion );

    void IDeclareApiVersionConventionBuilder.AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) => AdvertisesDeprecatedApiVersion( apiVersion );

    IActionConventionBuilder IControllerConventionBuilder.Action( MethodInfo actionMethod ) => Action( actionMethod );
}