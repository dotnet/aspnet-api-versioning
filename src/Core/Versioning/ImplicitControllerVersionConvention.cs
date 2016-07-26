namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using ApplicationModels;
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents an <see cref="IApplicationModelConvention">application model convention</see> which implicitly versions controllers.
    /// </summary>
    /// <remarks>This convention is used to apply a default service <see cref="ApiVersionModel">API version model</see> to controllers
    /// that do have explicit version information, which is particularly useful for existing services.</remarks>
    [CLSCompliant( false )]
    public sealed class ImplicitControllerVersionConvention : IApplicationModelConvention
    {
        private readonly ApiVersionModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplicitControllerVersionConvention"/> class.
        /// </summary>
        /// <param name="declaredVersion">The declared service <see cref="ApiVersion">API version</see> applied to controllers
        /// that do not have explicit versions.</param>
        public ImplicitControllerVersionConvention( ApiVersion declaredVersion )
        {
            Arg.NotNull( declaredVersion, nameof( declaredVersion ) );
            model = new ApiVersionModel( declaredVersion );
        }

        /// <summary>
        /// Applies the convention to the specified application.
        /// </summary>
        /// <param name="application">The <see cref="ApplicationModel">application</see> to apply the convention to.</param>
        public void Apply( ApplicationModel application )
        {
            foreach ( var controller in application.Controllers.Where( c => !c.HasExplicitVersioning() ) )
            {
                controller.SetProperty( model );

                foreach ( var action in controller.Actions )
                {
                    action.SetProperty( model );
                }
            }
        }
    }
}
