namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Linq;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial class VersionByNamespaceConvention : IControllerConvention
    {
        /// <summary>
        /// Applies a controller convention given the specified builder and model.
        /// </summary>
        /// <param name="controller">The <see cref="IControllerConventionBuilder">builder</see> used to apply conventions.</param>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">descriptor</see> to build conventions from.</param>
        /// <returns>True if any conventions were applied to the <paramref name="controllerDescriptor">descriptor</paramref>;
        /// otherwise, false.</returns>
        public virtual bool Apply( IControllerConventionBuilder controller, HttpControllerDescriptor controllerDescriptor )
        {
            Arg.NotNull( controller, nameof( controller ) );
            Arg.NotNull( controllerDescriptor, nameof( controller ) );

            var text = GetRawApiVersion( controllerDescriptor.ControllerType.Namespace );

            if ( !ApiVersion.TryParse( text, out var apiVersion ) )
            {
                return false;
            }

            var deprecated = controllerDescriptor.GetCustomAttributes<ObsoleteAttribute>().Any();

            if ( deprecated )
            {
                controller.HasDeprecatedApiVersion( apiVersion );
            }
            else
            {
                controller.HasApiVersion( apiVersion );
            }

            return true;
        }
    }
}