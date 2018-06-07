namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Web.Http.Controllers;

    /// <summary>
    /// Defines the behavior of a controller convention.
    /// </summary>
    public interface IControllerConvention
    {
        /// <summary>
        /// Applies a controller convention given the specified builder and model.
        /// </summary>
        /// <param name="controller">The <see cref="IControllerConventionBuilder">builder</see> used to apply conventions.</param>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">descriptor</see> to build conventions from.</param>
        /// <returns>True if any conventions were applied to the <paramref name="controllerDescriptor">descriptor</paramref>;
        /// otherwise, false.</returns>
        bool Apply( IControllerConventionBuilder controller, HttpControllerDescriptor controllerDescriptor );
    }
}