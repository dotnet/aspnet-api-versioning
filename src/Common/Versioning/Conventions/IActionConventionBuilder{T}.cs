#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Reflection;
#if WEBAPI
    using System.Web.Http.Controllers;
#endif

    /// <summary>
    /// Defines the behavior of a convention builder for a controller action.
    /// </summary>
    /// <typeparam name="T">The type of item the convention builder is for.</typeparam>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public interface IActionConventionBuilder<out T> : IMapToApiVersionConventionBuilder
#if WEBAPI
        where T : IHttpController
#endif
    {
        /// <summary>
        /// Gets or creates a convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The controller action <see cref="MethodInfo">method</see>
        /// to get or create a convention for.</param>
        /// <returns>A new or existing <see cref="IActionConventionBuilder{T}"/>.</returns>
        IActionConventionBuilder<T> Action( MethodInfo actionMethod );
    }
}