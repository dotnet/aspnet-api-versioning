#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;

    /// <summary>
    /// Defines the behavior of an API version convention builder.
    /// </summary>
    public partial interface IApiVersionConventionBuilder
    {
        /// <summary>
        /// Gets the count of configured conventions.
        /// </summary>
        /// <value>The total count of configured conventions.</value>
        int Count { get; }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller.
        /// </summary>
        /// <param name="controllerType">The <see cref="Type">type</see> of controller to build conventions for.</param>
        /// <returns>A new or existing <see cref="IControllerConventionBuilder"/>.</returns>
        IControllerConventionBuilder Controller( Type controllerType );

        /// <summary>
        /// Adds a new convention applied to all controllers.
        /// </summary>
        /// <param name="convention">The <see cref="IControllerConvention">convention</see> to be applied.</param>
        void Add( IControllerConvention convention );
    }
}