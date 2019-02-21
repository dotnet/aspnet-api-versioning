namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;

    /// <summary>
    /// Defines the behavior of a controller convention.
    /// </summary>
    [CLSCompliant(false)]
    public interface IControllerConvention
    {
        /// <summary>
        /// Applies a controller convention given the specified builder and model.
        /// </summary>
        /// <param name="controller">The <see cref="IControllerConventionBuilder">builder</see> used to apply conventions.</param>
        /// <param name="controllerModel">The <see cref="ControllerModel">model</see> to build conventions from.</param>
        /// <returns>True if any conventions were applied to the <paramref name="controllerModel">controller model</paramref>;
        /// otherwise, false.</returns>
        bool Apply( IControllerConventionBuilder controller, ControllerModel controllerModel );
    }
}