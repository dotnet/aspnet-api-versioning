namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ApplicationModels;
    using Conventions;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an <see cref="IApplicationModelConvention">application model convention</see> which applies
    /// convention-based API versions controllers and their actions.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersionConvention : IApplicationModelConvention
    {
        readonly ApiVersionConventionBuilder conventionBuilder;
        readonly ApiVersionModel implicitVersionModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionConvention"/> class.
        /// </summary>
        public ApiVersionConvention()
        {
            implicitVersionModel = ApiVersionModel.Default;
            conventionBuilder = new ApiVersionConventionBuilder();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionConvention"/> class.
        /// </summary>
        /// <param name="implicitlyDeclaredVersion">The implicitly declared <see cref="ApiVersion">API version</see> for
        /// controllers and actions that have no other API versioning information applied.</param>
        public ApiVersionConvention( ApiVersion implicitlyDeclaredVersion )
        {
            Arg.NotNull( implicitlyDeclaredVersion, nameof( implicitlyDeclaredVersion ) );

            implicitVersionModel = new ApiVersionModel( implicitlyDeclaredVersion );
            conventionBuilder = new ApiVersionConventionBuilder();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionConvention"/> class.
        /// </summary>
        /// <param name="implicitlyDeclaredVersion">The implicitly declared <see cref="ApiVersion">API version</see> for
        /// controllers and actions that have no other API versioning information applied.</param>
        /// <param name="conventionBuilder">The <see cref="ApiVersionConventionBuilder">convention builder</see>
        /// containing the configured conventions to apply.</param>
        public ApiVersionConvention( ApiVersion implicitlyDeclaredVersion, ApiVersionConventionBuilder conventionBuilder )
        {
            Arg.NotNull( implicitlyDeclaredVersion, nameof( implicitlyDeclaredVersion ) );
            Arg.NotNull( conventionBuilder, nameof( conventionBuilder ) );

            implicitVersionModel = new ApiVersionModel( implicitlyDeclaredVersion );
            this.conventionBuilder = conventionBuilder;
        }

        /// <summary>
        /// Applies the convention to the specified application.
        /// </summary>
        /// <param name="application">The <see cref="ApplicationModel">application</see> to apply the convention to.</param>
        public void Apply( ApplicationModel application )
        {
            if ( conventionBuilder.Count == 0 )
            {
                foreach ( var controller in application.Controllers )
                {
                    ApplyAttributeOrImplicitConventions( controller );
                }
            }
            else
            {
                foreach ( var controller in application.Controllers )
                {
                    if ( !conventionBuilder.ApplyTo( controller ) )
                    {
                        ApplyAttributeOrImplicitConventions( controller );
                    }
                }
            }
        }

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

        void ApplyImplicitConventions( ControllerModel controller )
        {
            Contract.Requires( controller != null );

            controller.SetProperty( implicitVersionModel );

            foreach ( var action in controller.Actions )
            {
                action.SetProperty( implicitVersionModel );
            }
        }

        void ApplyAttributeOrImplicitConventions( ControllerModel controller )
        {
            Contract.Requires( controller != null );

            if ( IsDecoratedWithAttributes( controller ) )
            {
                var conventions = new ControllerApiVersionConventionBuilder<ControllerModel>();
                conventions.ApplyTo( controller );
            }
            else
            {
                ApplyImplicitConventions( controller );
            }
        }
    }
}