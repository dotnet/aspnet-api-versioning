namespace Microsoft.AspNetCore.Mvc
{
    using ApplicationModels;
    using System;
    using Versioning;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class ApiVersionAttribute : IControllerModelConvention
    {
        /// <summary>
        /// Applies the conventions to the specified controller model.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerModel">controller model</see> to apply the conventions to.</param>
        public void Apply( ControllerModel controller )
        {
            controller.SetProperty( new ApiVersionModel( controller ) );

            foreach ( var action in controller.Actions )
            {
                action.SetProperty( controller );
                action.SetProperty( new ApiVersionModel( controller, action ) );
            }
        }
    }
}
