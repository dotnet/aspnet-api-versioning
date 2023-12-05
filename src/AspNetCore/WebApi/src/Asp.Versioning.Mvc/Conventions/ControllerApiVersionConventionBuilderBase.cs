// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Reflection;

/// <summary>
/// Represents the base implementation of a builder for API versions applied to a controller.
/// </summary>
[CLSCompliant( false )]
public abstract class ControllerApiVersionConventionBuilderBase : ApiVersionConventionBuilderBase, IApiVersionConvention<ControllerModel>
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
    /// Gets the controller naming convention associated with the builder.
    /// </summary>
    /// <value>The <see cref="IControllerNameConvention">controller naming convention</see>.</value>
    public IControllerNameConvention NamingConvention { get; }

    /// <summary>
    /// Applies the builder conventions to the specified controller.
    /// </summary>
    /// <param name="item">The <see cref="ControllerModel">controller model</see> to apply the conventions to.</param>
    public virtual void ApplyTo( ControllerModel item )
    {
        ArgumentNullException.ThrowIfNull( item );
        MergeAttributesWithConventions( item.Attributes );
        ApplyActionConventions( item );
    }

    /// <summary>
    /// Attempts to get the convention for the specified action method.
    /// </summary>
    /// <param name="method">The <see cref="MethodInfo">method</see> representing the action to retrieve the convention for.</param>
    /// <param name="convention">The retrieved <see cref="IApiVersionConvention{T}">convention</see> or <c>null</c>.</param>
    /// <returns>True if the convention was successfully retrieved; otherwise, false.</returns>
    protected abstract bool TryGetConvention( MethodInfo method, [MaybeNullWhen( false )] out IApiVersionConvention<ActionModel> convention );

    private void ApplyActionConventions( ControllerModel controller )
    {
        var actions = controller.Actions;

        if ( VersionNeutral )
        {
            var name = NamingConvention.GroupName( controller.ControllerName );
            var metadata = string.IsNullOrEmpty( name ) ?
                           ApiVersionMetadata.Neutral :
                           new ApiVersionMetadata( ApiVersionModel.Neutral, ApiVersionModel.Neutral, name );

            for ( var i = 0; i < actions.Count; i++ )
            {
                actions[i].AddEndpointMetadata( metadata );
            }

            return;
        }

        var apiModel = new ApiVersionModel(
            declaredVersions: SupportedVersions.Union( DeprecatedVersions ),
            SupportedVersions,
            DeprecatedVersions,
            AdvertisedVersions,
            DeprecatedAdvertisedVersions );
        var controllerBuilder = new ControllerApiVersionConventionBuilder( controller.ControllerType, NamingConvention );

        controller.Properties[typeof( ApiVersionModel )] = apiModel;

        for ( var i = 0; i < actions.Count; i++ )
        {
            var action = actions[i];
            var key = action.ActionMethod;

            if ( !TryGetConvention( key, out var actionConvention ) )
            {
                actionConvention = new ActionApiVersionConventionBuilder( controllerBuilder );
            }

            actionConvention.ApplyTo( action );
        }

        controller.Properties.Remove( typeof( ApiVersionModel ) );
    }
}