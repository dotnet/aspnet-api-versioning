namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Represents the base implementation of a builder for API versions applied to a controller.
    /// </summary>
    [CLSCompliant( false )]
    public abstract class ControllerApiVersionConventionBuilderBase : ApiVersionConventionBuilderBase, IApiVersionConvention<ControllerModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilderBase"/> class.
        /// </summary>
        protected ControllerApiVersionConventionBuilderBase() { }

        /// <summary>
        /// Applies the builder conventions to the specified controller.
        /// </summary>
        /// <param name="controllerModel">The <see cref="ControllerModel">controller model</see> to apply the conventions to.</param>
        public virtual void ApplyTo( ControllerModel controllerModel )
        {
            if ( controllerModel == null )
            {
                throw new ArgumentNullException( nameof( controllerModel ) );
            }

            MergeAttributesWithConventions( controllerModel.Attributes );
            ApplyActionConventions( controllerModel );
        }

        /// <summary>
        /// Attempts to get the convention for the specified action method.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo">method</see> representing the action to retrieve the convention for.</param>
        /// <param name="convention">The retrieved <see cref="IApiVersionConvention{T}">convention</see> or <c>null</c>.</param>
        /// <returns>True if the convention was successfully retrieved; otherwise, false.</returns>
        protected abstract bool TryGetConvention( MethodInfo method, out IApiVersionConvention<ActionModel>? convention );

        void ApplyActionConventions( ControllerModel controller )
        {
            if ( VersionNeutral )
            {
                controller.SetProperty( ApiVersionModel.Neutral );

                for ( var i = 0; i < controller.Actions.Count; i++ )
                {
                    var action = controller.Actions[i];

                    action.SetProperty( controller );
                    action.SetProperty( ApiVersionModel.Neutral );
                }

                return;
            }

            controller.SetProperty( new ApiVersionModel( SupportedVersions, DeprecatedVersions, AdvertisedVersions, DeprecatedAdvertisedVersions ) );

            var anyController = new ControllerApiVersionConventionBuilder( typeof( ControllerBase ) );

            for ( var i = 0; i < controller.Actions.Count; i++ )
            {
                var action = controller.Actions[i];
                var key = action.ActionMethod;

                if ( !TryGetConvention( key, out var actionConvention ) )
                {
                    actionConvention = new ActionApiVersionConventionBuilder( anyController );
                }

                action.SetProperty( controller );
                actionConvention!.ApplyTo( action );
            }

            ApplyInheritedActionConventions( controller.Actions );
        }

        void ApplyInheritedActionConventions( IList<ActionModel> actions )
        {
            var noInheritedApiVersions = SupportedVersions.Count == 0 && DeprecatedVersions.Count == 0 && AdvertisedVersions.Count == 0 && DeprecatedAdvertisedVersions.Count == 0;

            if ( noInheritedApiVersions )
            {
                return;
            }

            for ( var i = 0; i < actions.Count; i++ )
            {
                var action = actions[i];
                var model = action.GetProperty<ApiVersionModel>();

                if ( model == null || model.IsApiVersionNeutral )
                {
                    continue;
                }

                var supportedVersions = model.SupportedApiVersions.Union( SupportedVersions );
                var deprecatedVersions = model.DeprecatedApiVersions.Union( DeprecatedVersions );

                model = new ApiVersionModel(
                   declaredVersions: model.DeclaredApiVersions,
                   supportedVersions,
                   deprecatedVersions,
                   AdvertisedVersions,
                   DeprecatedAdvertisedVersions );

                action.SetProperty( model );
            }
        }
    }
}