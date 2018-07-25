﻿namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.Extensions.Options;
    using System;
    using System.Diagnostics.Contracts;

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
        public ApiVersioningApplicationModelProvider( IOptions<ApiVersioningOptions> options )
        {
            Arg.NotNull( options, nameof( options ) );
            this.options = options;
        }

        /// <summary>
        /// Gets the API versioning options associated with the model provider.
        /// </summary>
        /// <value>The current <see cref="ApiVersioningOptions">API versioning options</see>.</value>
        protected ApiVersioningOptions Options => options.Value;

        /// <inheritdoc />
        public int Order { get; protected set; }

        /// <inheritdoc />
        public virtual void OnProvidersExecuted( ApplicationModelProviderContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            var implicitVersionModel = new ApiVersionModel( Options.DefaultApiVersion );
            var conventionBuilder = Options.Conventions;
            var application = context.Result;

            if ( conventionBuilder.Count == 0 )
            {
                foreach ( var controller in application.Controllers )
                {
                    ApplyAttributeOrImplicitConventions( controller, implicitVersionModel );
                }
            }
            else
            {
                foreach ( var controller in application.Controllers )
                {
                    if ( !conventionBuilder.ApplyTo( controller ) )
                    {
                        ApplyAttributeOrImplicitConventions( controller, implicitVersionModel );
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnProvidersExecuting( ApplicationModelProviderContext context ) { }

        static bool IsDecoratedWithAttributes( ControllerModel controller )
        {
            Contract.Requires( controller != null );

            foreach ( var attribute in controller.Attributes )
            {
                if ( attribute is IApiVersionProvider || attribute is IApiVersionNeutral )
                {
                    return true;
                }
            }

            return false;
        }

        static void ApplyImplicitConventions( ControllerModel controller, ApiVersionModel implicitVersionModel )
        {
            Contract.Requires( controller != null );
            Contract.Requires( implicitVersionModel != null );

            controller.SetProperty( implicitVersionModel );

            foreach ( var action in controller.Actions )
            {
                action.SetProperty( implicitVersionModel );
            }
        }

        static void ApplyAttributeOrImplicitConventions( ControllerModel controller, ApiVersionModel implicitVersionModel )
        {
            Contract.Requires( controller != null );
            Contract.Requires( implicitVersionModel != null );

            if ( IsDecoratedWithAttributes( controller ) )
            {
                var conventions = new ControllerApiVersionConventionBuilder<ControllerModel>();
                conventions.ApplyTo( controller );
            }
            else
            {
                ApplyImplicitConventions( controller, implicitVersionModel );
            }
        }
    }
}