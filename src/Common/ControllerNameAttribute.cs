#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using static System.AttributeTargets;

    /// <summary>
    /// Represents the metadata to describe the name of a controller.
    /// </summary>
    /// <remarks>This attribute is required to support service versioning on ASP.NET controllers that use
    /// convention-based routing because the route is inferred from the type name and service API versions
    /// may be implemented using different controller types.</remarks>
    [AttributeUsage( Class, AllowMultiple = false, Inherited = false )]
    public sealed partial class ControllerNameAttribute
    {
        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        /// <value>The controller name.</value>
        public string Name { get; }
    }
}