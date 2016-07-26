namespace Microsoft.AspNetCore.Mvc
{
    using ApplicationModels;
    using System;
    using static Versioning.ApiVersionModel;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class ApiVersionNeutralAttribute : IControllerModelConvention
    {
        /// <summary>
        /// Applies the conventions to the specified controller model.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerModel">controller model</see> to apply the conventions to.</param>
        public void Apply( ControllerModel controller )
        {
            var model = Neutral;

            controller.SetProperty( model );

            foreach ( var action in controller.Actions )
            {
                action.SetProperty( model );
            }
        }
    }
}
