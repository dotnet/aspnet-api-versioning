namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial interface IApiVersionConventionBuilder
    {
        /// <summary>
        /// Gets or creates the convention builder for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The <see cref="Type">type</see> of controller to build conventions for.</typeparam>
        /// <returns>A new or existing <see cref="IControllerConventionBuilder{T}"/>.</returns>
        IControllerConventionBuilder<TController> Controller<TController>() where TController : notnull;

        /// <summary>
        /// Applies the defined API version conventions to the specified controller.
        /// </summary>
        /// <param name="controllerModel">The <see cref="ControllerModel">controller model</see>
        /// to apply configured conventions to.</param>
        /// <returns>True if any conventions were applied to the
        /// <paramref name="controllerModel">controller model</paramref>; otherwise, false.</returns>
        bool ApplyTo( ControllerModel controllerModel );
    }
}