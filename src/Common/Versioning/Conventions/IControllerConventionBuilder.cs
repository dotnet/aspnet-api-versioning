#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Defines the behavior of a convention builder for a controller.
    /// </summary>
    public partial interface IControllerConventionBuilder : IDeclareApiVersionConventionBuilder
    {
        /// <summary>
        /// Gets the type of controller the convention builder is for.
        /// </summary>
        /// <value>The corresponding controller <see cref="Type">type</see>.</value>
        Type ControllerType { get; }

        /// <summary>
        /// Gets or creates a convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The controller action <see cref="MethodInfo">method</see>
        /// to get or create a convention for.</param>
        /// <returns>A new or existing <see cref="IActionConventionBuilder"/>.</returns>
        IActionConventionBuilder Action( MethodInfo actionMethod );
    }
}