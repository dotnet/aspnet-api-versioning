namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial interface IApiVersionConventionBuilder
    {
        /// <summary>
        /// Gets or creates the convention builder for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The <see cref="Type">type</see> of controller to build conventions for.</typeparam>
        /// <returns>A new or existing <see cref="IControllerConventionBuilder{T}"/>.</returns>
        IControllerConventionBuilder<TController> Controller<TController>() where TController : IHttpController;

        /// <summary>
        /// Applies the defined API version conventions to the specified controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller descriptor</see>
        /// to apply configured conventions to.</param>
        /// <returns>True if any conventions were applied to the
        /// <paramref name="controllerDescriptor">controller descriptor</paramref>; otherwise, false.</returns>
        bool ApplyTo( HttpControllerDescriptor controllerDescriptor );
    }
}