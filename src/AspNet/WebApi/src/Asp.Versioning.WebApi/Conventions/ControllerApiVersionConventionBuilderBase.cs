// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;

/// <summary>
/// Represents the base implementation of a builder for API versions applied to a controller.
/// </summary>
public abstract class ControllerApiVersionConventionBuilderBase : ApiVersionConventionBuilderBase, IApiVersionConvention<HttpControllerDescriptor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilderBase"/> class.
    /// </summary>
    protected ControllerApiVersionConventionBuilderBase() => NamingConvention = ControllerNameConvention.Default;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilderBase"/> class.
    /// </summary>
    /// <param name="namingConvention">The <see cref="IControllerNameConvention">controller naming convention</see>.</param>
    protected ControllerApiVersionConventionBuilderBase( IControllerNameConvention namingConvention ) => NamingConvention = namingConvention;

    /// <summary>
    /// Gets the controller naming convention associated with the collator.
    /// </summary>
    /// <value>The <see cref="IControllerNameConvention">controller naming convention</see>.</value>
    public IControllerNameConvention NamingConvention { get; }

    /// <summary>
    /// Applies the builder conventions to the specified controller.
    /// </summary>
    /// <param name="item">The <see cref="HttpControllerDescriptor">controller descriptor</see>
    /// to apply the conventions to.</param>
    public virtual void ApplyTo( HttpControllerDescriptor item )
    {
        ArgumentNullException.ThrowIfNull( item );

        var attributes = new List<object>();

        attributes.AddRange( item.GetCustomAttributes<IApiVersionNeutral>( inherit: true ) );
        attributes.AddRange( item.GetCustomAttributes<IApiVersionProvider>( inherit: false ) );
        MergeAttributesWithConventions( attributes );
        ApplyActionConventions( item );
    }

    /// <summary>
    /// Attempts to get the convention for the specified action method.
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo">method</see> representing the action to retrieve the convention for.</param>
    /// <param name="convention">The retrieved <see cref="IApiVersionConvention{T}">convention</see> or <c>null</c>.</param>
    /// <returns>True if the convention was successfully retrieved; otherwise, false.</returns>
    protected abstract bool TryGetConvention( MethodInfo method, [MaybeNullWhen( false )] out IApiVersionConvention<HttpActionDescriptor> convention );

    private void ApplyActionConventions( HttpControllerDescriptor controller )
    {
        var actionSelector = controller.Configuration.Services.GetActionSelector();
        var actions = actionSelector.GetActionMapping( controller ).SelectMany( g => g ).ToArray();

        if ( VersionNeutral )
        {
            var name = NamingConvention.GroupName( controller.ControllerName );
            var metadata = string.IsNullOrEmpty( name ) ?
                           ApiVersionMetadata.Neutral :
                           new ApiVersionMetadata( ApiVersionModel.Neutral, ApiVersionModel.Neutral, name );

            controller.SetApiVersionModel( ApiVersionModel.Neutral );

            for ( var i = 0; i < actions.Length; i++ )
            {
                actions[i].SetApiVersionMetadata( metadata );
            }

            return;
        }

        controller.SetApiVersionModel(
            new ApiVersionModel(
                SupportedVersions,
                DeprecatedVersions,
                AdvertisedVersions,
                DeprecatedAdvertisedVersions ) );

        var controllerBuilder = new ControllerApiVersionConventionBuilder( controller.ControllerType );

        for ( var i = 0; i < actions.Length; i++ )
        {
            if ( actions[i] is not ReflectedHttpActionDescriptor action )
            {
                continue;
            }

            var key = action.MethodInfo;

            if ( !TryGetConvention( key, out var actionConvention ) )
            {
                actionConvention = new ActionApiVersionConventionBuilder( controllerBuilder );
            }

            actionConvention!.ApplyTo( action );
        }
    }
}