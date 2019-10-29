namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using static System.Linq.Enumerable;

    /// <summary>
    /// Represents the base implementation of a builder for API versions applied to a controller.
    /// </summary>
    public abstract class ControllerApiVersionConventionBuilderBase : ApiVersionConventionBuilderBase, IApiVersionConvention<HttpControllerDescriptor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilderBase"/> class.
        /// </summary>
        protected ControllerApiVersionConventionBuilderBase() { }

        /// <summary>
        /// Applies the builder conventions to the specified controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller descriptor</see>
        /// to apply the conventions to.</param>
        public virtual void ApplyTo( HttpControllerDescriptor controllerDescriptor )
        {
            if ( controllerDescriptor == null )
            {
                throw new ArgumentNullException( nameof( controllerDescriptor ) );
            }

            var attributes = new List<object>();

            attributes.AddRange( controllerDescriptor.GetCustomAttributes<IApiVersionNeutral>( inherit: true ) );
            attributes.AddRange( controllerDescriptor.GetCustomAttributes<IApiVersionProvider>( inherit: false ) );
            MergeAttributesWithConventions( attributes );
            ApplyActionConventions( controllerDescriptor );
        }

        /// <summary>
        /// Gets a value indicating whether the builder has any related action conventions.
        /// </summary>
        /// <value>True if the builder has related action conventions; otherwise, false.</value>
        protected abstract bool HasActionConventions { get; }

        /// <summary>
        /// Attempts to get the convention for the specified action method.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo">method</see> representing the action to retrieve the convention for.</param>
        /// <param name="convention">The retrieved <see cref="IApiVersionConvention{T}">convention</see> or <c>null</c>.</param>
        /// <returns>True if the convention was successfully retrieved; otherwise, false.</returns>
        protected abstract bool TryGetConvention( MethodInfo method, out IApiVersionConvention<HttpActionDescriptor>? convention );

        void ApplyActionConventions( HttpControllerDescriptor controller )
        {
            var actionSelector = controller.Configuration.Services.GetActionSelector();
            var actions = actionSelector.GetActionMapping( controller ).SelectMany( g => g ).ToArray();

            if ( VersionNeutral )
            {
                controller.SetApiVersionModel( ApiVersionModel.Neutral );

                for ( var i = 0; i < actions.Length; i++ )
                {
                    actions[i].SetProperty( ApiVersionModel.Neutral );
                }

                return;
            }

            controller.SetApiVersionModel( new ApiVersionModel( SupportedVersions, DeprecatedVersions, AdvertisedVersions, DeprecatedAdvertisedVersions ) );

            var anyController = new ControllerApiVersionConventionBuilder( typeof( IHttpController ) );

            for ( var i = 0; i < actions.Length; i++ )
            {
                if ( !( actions[i] is ReflectedHttpActionDescriptor action ) )
                {
                    continue;
                }

                var key = action.MethodInfo;

                if ( !TryGetConvention( key, out var actionConvention ) )
                {
                    actionConvention = new ActionApiVersionConventionBuilder( anyController );
                }

                actionConvention!.ApplyTo( action );
            }

            ApplyInheritedActionConventions( actions );
        }

        void ApplyInheritedActionConventions( IReadOnlyList<HttpActionDescriptor> actions )
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