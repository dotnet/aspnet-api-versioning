namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// Represents an <see cref="IApplicationModelProvider">application model provider</see>, which
    /// applies convention-based API versions controllers and their actions.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersioningApplicationModelProvider : IApplicationModelProvider
    {
        readonly IOptions<ApiVersioningOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersioningApplicationModelProvider"/> class.
        /// </summary>
        /// <param name="options">The current <see cref="ApiVersioningOptions">API versioning options</see>.</param>
        /// <param name="controllerFilter">The <see cref="IApiControllerFilter">filter</see> used for API controllers.</param>
        public ApiVersioningApplicationModelProvider( IOptions<ApiVersioningOptions> options, IApiControllerFilter controllerFilter )
        {
            this.options = options;
            ControllerFilter = controllerFilter;
        }

        /// <summary>
        /// Gets the API versioning options associated with the model provider.
        /// </summary>
        /// <value>The current <see cref="ApiVersioningOptions">API versioning options</see>.</value>
        protected ApiVersioningOptions Options => options.Value;

        /// <summary>
        /// Gets the filter used for API controllers.
        /// </summary>
        /// <value>The <see cref="IApiControllerFilter"/> used to filter API controllers.</value>
        protected IApiControllerFilter ControllerFilter { get; }

        /// <inheritdoc />
        public int Order { get; protected set; }

        /// <inheritdoc />
        public virtual void OnProvidersExecuted( ApplicationModelProviderContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var implicitVersionModel = new ApiVersionModel( Options.DefaultApiVersion );
            var conventionBuilder = Options.Conventions;
            var application = context.Result;
            var controllers = application.Controllers;

            if ( Options.UseApiBehavior )
            {
                controllers = ControllerFilter.Apply( controllers );
            }

            for ( var i = 0; i < controllers.Count; i++ )
            {
                var controller = controllers[i];

                if ( !conventionBuilder.ApplyTo( controller ) )
                {
                    ApplyAttributeOrImplicitConventions( controller, implicitVersionModel );
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnProvidersExecuting( ApplicationModelProviderContext context ) { }

        static bool IsDecoratedWithAttributes( ControllerModel controller )
        {
            for ( var i = 0; i < controller.Attributes.Count; i++ )
            {
                var attribute = controller.Attributes[i];

                if ( attribute is IApiVersionProvider || attribute is IApiVersionNeutral )
                {
                    return true;
                }
            }

            return false;
        }

        static void ApplyImplicitConventions( ControllerModel controller, ApiVersionModel implicitVersionModel )
        {
            for ( var i = 0; i < controller.Actions.Count; i++ )
            {
                var action = controller.Actions[i];
                action.SetProperty( controller );
                action.SetProperty( implicitVersionModel );
            }
        }

        static void ApplyAttributeOrImplicitConventions( ControllerModel controller, ApiVersionModel implicitVersionModel )
        {
            if ( IsDecoratedWithAttributes( controller ) )
            {
                var conventions = new ControllerApiVersionConventionBuilder( controller.ControllerType );
                conventions.ApplyTo( controller );
            }
            else
            {
                ApplyImplicitConventions( controller, implicitVersionModel );
            }
        }
    }
}