namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;
    using System.Linq;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class VersionByNamespaceConvention : IControllerConvention
    {
        /// <summary>
        /// Applies a controller convention given the specified builder and model.
        /// </summary>
        /// <param name="controller">The <see cref="IControllerConventionBuilder">builder</see> used to apply conventions.</param>
        /// <param name="controllerModel">The <see cref="ControllerModel">model</see> to build conventions from.</param>
        /// <returns>True if any conventions were applied to the <paramref name="controllerModel">controller model</paramref>;
        /// otherwise, false.</returns>
        public virtual bool Apply( IControllerConventionBuilder controller, ControllerModel controllerModel )
        {
            if ( controller == null )
            {
                throw new ArgumentNullException( nameof( controller ) );
            }

            if ( controllerModel == null )
            {
                throw new ArgumentNullException( nameof( controllerModel ) );
            }

            var text = GetRawApiVersion( controllerModel.ControllerType.Namespace! );

            if ( !ApiVersion.TryParse( text, out var apiVersion ) )
            {
                return false;
            }

            var deprecated = controllerModel.Attributes.OfType<ObsoleteAttribute>().Any();

            if ( deprecated )
            {
                controller.HasDeprecatedApiVersion( apiVersion! );
            }
            else
            {
                controller.HasApiVersion( apiVersion! );
            }

            return true;
        }
    }
}