#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;

    /// <summary>
    ///  Defines the behavior of a convention for controller names.
    /// </summary>
    public interface IControllerNameConvention
    {
        /// <summary>
        /// Normalizes the specified controller name.
        /// </summary>
        /// <param name="controllerName">The name of the controller.</param>
        /// <returns>The normalized name of the specified controller.</returns>
        string NormalizeName( string controllerName );

        /// <summary>
        /// Gets the name used for grouping the specified controller.
        /// </summary>
        /// <param name="controllerName">The name of the controller.</param>
        /// <returns>The group name of the specified controller.</returns>
        string GroupName( string controllerName );
    }
}